using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using AOT;
using Newtonsoft.Json;

namespace HybridWebSocket
{
    public delegate void WebSocketOpenEventHandler();

    public delegate void WebSocketMessageEventHandler(byte[] data);

    public delegate void WebSocketErrorEventHandler(string errorMsg);

    public delegate void WebSocketCloseEventHandler(WebSocketCloseCode closeCode);

    public enum WebSocketState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    public enum WebSocketCloseCode
    {
        /* Do NOT use NotSet - it's only purpose is to indicate that the close code cannot be parsed. */
        NotSet = 0,
        Normal = 1000,
        Away = 1001,
        ProtocolError = 1002,
        UnsupportedData = 1003,
        Undefined = 1004,
        NoStatus = 1005,
        Abnormal = 1006,
        InvalidData = 1007,
        PolicyViolation = 1008,
        TooBig = 1009,
        MandatoryExtension = 1010,
        ServerError = 1011,
        TlsHandshakeFailure = 1015
    }

    public interface IWebSocket
    {
        void Connect();
        void SubscribeToNewHeads();
        void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null);
        void Send(byte[] data);
        WebSocketState GetState();
        event WebSocketOpenEventHandler OnOpen;
        event WebSocketMessageEventHandler OnMessage;
        event WebSocketErrorEventHandler OnError;
        event WebSocketCloseEventHandler OnClose;
    }

    public static class WebSocketHelpers
    {
        public static WebSocketCloseCode ParseCloseCodeEnum(int closeCode)
        {
            if (WebSocketCloseCode.IsDefined(typeof(WebSocketCloseCode), closeCode))
            {
                return (WebSocketCloseCode)closeCode;
            }
            else
            {
                return WebSocketCloseCode.Undefined;
            }
        }

        public static WebSocketException GetErrorMessageFromCode(int errorCode, Exception inner)
        {
            switch (errorCode)
            {
                case -1: return new WebSocketUnexpectedException("WebSocket instance not found.", inner);
                case -2:
                    return new WebSocketInvalidStateException("WebSocket is already connected or in connecting state.",
                        inner);
                case -3: return new WebSocketInvalidStateException("WebSocket is not connected.", inner);
                case -4: return new WebSocketInvalidStateException("WebSocket is already closing.", inner);
                case -5: return new WebSocketInvalidStateException("WebSocket is already closed.", inner);
                case -6: return new WebSocketInvalidStateException("WebSocket is not in open state.", inner);
                case -7:
                    return new WebSocketInvalidArgumentException(
                        "Cannot close WebSocket. An invalid code was specified or reason is too long.", inner);
                default: return new WebSocketUnexpectedException("Unknown error.", inner);
            }
        }
    }

    public class WebSocketException : Exception
    {
        public WebSocketException()
        {
        }

        public WebSocketException(string message)
            : base(message)
        {
        }

        public WebSocketException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class WebSocketUnexpectedException : WebSocketException
    {
        public WebSocketUnexpectedException()
        {
        }

        public WebSocketUnexpectedException(string message) : base(message)
        {
        }

        public WebSocketUnexpectedException(string message, Exception inner) : base(message, inner)
        {
        }
    }


    public class WebSocketInvalidArgumentException : WebSocketException
    {
        public WebSocketInvalidArgumentException()
        {
        }

        public WebSocketInvalidArgumentException(string message) : base(message)
        {
        }

        public WebSocketInvalidArgumentException(string message, Exception inner) : base(message, inner)
        {
        }
    }


    public class WebSocketInvalidStateException : WebSocketException
    {
        public WebSocketInvalidStateException()
        {
        }

        public WebSocketInvalidStateException(string message) : base(message)
        {
            Debug.Log(message);
        }

        public WebSocketInvalidStateException(string message, Exception inner) : base(message, inner)
        {
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    public class WebSocket: IWebSocket
    {

        /* WebSocket JSLIB functions */
        [DllImport("__Internal")]
        public static extern int WebSocketConnect(int instanceId);

        [DllImport("__Internal")]
        public static extern int WebSocketClose(int instanceId, int code, string reason);

        [DllImport("__Internal")]
        public static extern int WebSocketSend(int instanceId, byte[] dataPtr, int dataLength);

        [DllImport("__Internal")]
        public static extern int WebSocketGetState(int instanceId);

        protected int instanceId;
        public event WebSocketOpenEventHandler OnOpen;
        public event WebSocketMessageEventHandler OnMessage;
        public event WebSocketErrorEventHandler OnError;
        public event WebSocketCloseEventHandler OnClose;

        public WebSocket(int instanceId)
        {

            this.instanceId = instanceId;

        }

        ~WebSocket()
        {
            WebSocketFactory.HandleInstanceDestroy(this.instanceId);
        }

        public int GetInstanceId()
        {

            return this.instanceId;

        }

        public void Connect()
        {

            int ret = WebSocketConnect(this.instanceId);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

        }

        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {

            int ret = WebSocketClose(this.instanceId, (int)code, reason);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

        }

        public void Send(byte[] data)
        {

            int ret = WebSocketSend(this.instanceId, data, data.Length);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

        }


        public void SubscribeToNewHeads()
        {
                string typeStr = "newHeads";
                var subscriptionRequest = new
                {
                    jsonrpc = "2.0",
                    id = 1,
                    method = "eth_subscribe",
                    @params = new List<string> { typeStr }
                };

            string jsonString = JsonConvert.SerializeObject(subscriptionRequest);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            Send(byteArray);
        }

        public WebSocketState GetState()
        {

            int state = WebSocketGetState(this.instanceId);

            if (state < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(state, null);

            switch (state)
            {
                case 0:
                    return WebSocketState.Connecting;

                case 1:
                    return WebSocketState.Open;

                case 2:
                    return WebSocketState.Closing;

                case 3:
                    return WebSocketState.Closed;

                default:
                    return WebSocketState.Closed;
            }

        }

        public void DelegateOnOpenEvent()
        {

            this.OnOpen?.Invoke();

        }

        public void DelegateOnMessageEvent(byte[] data)
        {

            this.OnMessage?.Invoke(data);

        }

        public void DelegateOnErrorEvent(string errorMsg)
        {

            this.OnError?.Invoke(errorMsg);

        }

        public void DelegateOnCloseEvent(int closeCode)
        {

            this.OnClose?.Invoke(WebSocketHelpers.ParseCloseCodeEnum(closeCode));

        }

    }
#else

    public class WebSocket : IWebSocket
    {
        public event WebSocketOpenEventHandler OnOpen;

        public event WebSocketMessageEventHandler OnMessage;

        public event WebSocketErrorEventHandler OnError;

        public event WebSocketCloseEventHandler OnClose;


        protected WebSocketSharp.WebSocket ws;
        private enum SslProtocolsHack
        {
            Tls = 192,
            Tls11 = 768,
            Tls12 = 3072
        }

        public WebSocket(string url)
        {
            try
            {
                // Create WebSocket instance
                this.ws = new WebSocketSharp.WebSocket(url);

                // Bind OnOpen event
                this.ws.OnOpen += (sender, ev) => { this.OnOpen?.Invoke(); };

                // Bind OnMessage event
                this.ws.OnMessage += (sender, ev) =>
                {
                    if (ev.RawData != null)
                        this.OnMessage?.Invoke(ev.RawData);
                };

                // Bind OnError event
                this.ws.OnError += (sender, ev) => { this.OnError?.Invoke(ev.Message); };

                // Bind OnClose event
                this.ws.OnClose += (sender, ev) =>
                {
                    var sslProtocolHack = (System.Security.Authentication.SslProtocols)(SslProtocolsHack.Tls12 | SslProtocolsHack.Tls11 | SslProtocolsHack.Tls);
                    //TlsHandshakeFailure
                    if (ev.Code == 1015 && ws.SslConfiguration.EnabledSslProtocols != sslProtocolHack)
                    {
                        Debug.LogError("TlsHandshakeFailure");
                        ws.SslConfiguration.EnabledSslProtocols = sslProtocolHack;
                        ws.Connect();
                    } else {
                        this.OnClose?.Invoke(
                            WebSocketHelpers.ParseCloseCodeEnum((int)ev.Code)
                        );
                    }

                    
                };
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to create WebSocket Client.", e);
            }
        }

        public void Connect()
        {
            // Check state
            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Open ||
                this.ws.ReadyState == WebSocketSharp.WebSocketState.Closing)
                throw new WebSocketInvalidStateException("WebSocket is already connected or is closing.");

            try
            {
                this.ws.ConnectAsync();
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to connect.", e);
            }
        }

        public void SubscribeToNewHeads()
        {
            Debug.Log("nope");
            try
            {
                string typeStr = "newHeads"; // Substitute this with your actual value
                var subscriptionRequest = new
                {
                    jsonrpc = "2.0",
                    id = 1,
                    method = "eth_subscribe",
                    @params = new List<string> { typeStr }
                };
                
                this.ws.SendAsync(JsonConvert.SerializeObject(subscriptionRequest), (bool success) => Debug.Log("Sent!"));
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to connect.", e);
            }
        }


        public void Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            // Check state
            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Closing)
                throw new WebSocketInvalidStateException("WebSocket is already closing.");

            if (this.ws.ReadyState == WebSocketSharp.WebSocketState.Closed)
                throw new WebSocketInvalidStateException("WebSocket is already closed.");

            try
            {
                this.ws.CloseAsync((ushort)code, reason);
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to close the connection.", e);
            }
        }

        public void Send(byte[] data)
        {
            // Check state
            if (this.ws.ReadyState != WebSocketSharp.WebSocketState.Open)
                throw new WebSocketInvalidStateException("WebSocket is not in open state.");

            try
            {
                this.ws.Send(data);
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to send message.", e);
            }
        }

        public WebSocketState GetState()
        {
            switch (this.ws.ReadyState)
            {
                case WebSocketSharp.WebSocketState.Connecting:
                    return WebSocketState.Connecting;

                case WebSocketSharp.WebSocketState.Open:
                    return WebSocketState.Open;

                case WebSocketSharp.WebSocketState.Closing:
                    return WebSocketState.Closing;

                case WebSocketSharp.WebSocketState.Closed:
                    return WebSocketState.Closed;

                default:
                    return WebSocketState.Closed;
            }
        }
    }
#endif

    public static class WebSocketFactory
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        /* Map of websocket instances */
        private static Dictionary<Int32, WebSocket> instances = new Dictionary<Int32, WebSocket>();

        /* Delegates */
        public delegate void OnOpenCallback(int instanceId);
        public delegate void OnMessageCallback(int instanceId, System.IntPtr msgPtr, int msgSize);
        public delegate void OnErrorCallback(int instanceId, System.IntPtr errorPtr);
        public delegate void OnCloseCallback(int instanceId, int closeCode);

        /* WebSocket JSLIB callback setters and other functions */
        [DllImport("__Internal")]
        public static extern int WebSocketAllocate(string url);

        [DllImport("__Internal")]
        public static extern void WebSocketFree(int instanceId);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnClose(OnCloseCallback callback);

        /* If callbacks was initialized and set */
        private static bool isInitialized = false;

        /*
         * Initialize WebSocket callbacks to JSLIB
         */
        private static void Initialize()
        {

            WebSocketSetOnOpen(DelegateOnOpenEvent);
            WebSocketSetOnMessage(DelegateOnMessageEvent);
            WebSocketSetOnError(DelegateOnErrorEvent);
            WebSocketSetOnClose(DelegateOnCloseEvent);

            isInitialized = true;

        }

        public static void HandleInstanceDestroy(int instanceId)
        {

            instances.Remove(instanceId);
            WebSocketFree(instanceId);

        }

        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        public static void DelegateOnOpenEvent(int instanceId)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                instanceRef.DelegateOnOpenEvent();
            }

        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void DelegateOnMessageEvent(int instanceId, System.IntPtr msgPtr, int msgSize)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                byte[] msg = new byte[msgSize];
                Marshal.Copy(msgPtr, msg, 0, msgSize);

                instanceRef.DelegateOnMessageEvent(msg);
            }

        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        public static void DelegateOnErrorEvent(int instanceId, System.IntPtr errorPtr)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {

                string errorMsg = Marshal.PtrToStringAuto(errorPtr);
                instanceRef.DelegateOnErrorEvent(errorMsg);

            }

        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        public static void DelegateOnCloseEvent(int instanceId, int closeCode)
        {

            WebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                instanceRef.DelegateOnCloseEvent(closeCode);
            }

        }
#endif

        public static WebSocket CreateInstance(string url)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isInitialized)
            Initialize();

        int instanceId = WebSocketAllocate(url);
        WebSocket wrapper = new WebSocket(instanceId);
        instances.Add(instanceId, wrapper);

        return wrapper;
#else
            return new WebSocket(url);
#endif
        }
    }
}
