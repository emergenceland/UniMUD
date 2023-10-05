import { exec } from "child_process";
import fs from "fs";
import path from "path";

interface GeneratorConfig {
  ContractName?: string;
  ABI?: string;
  ABIFile?: string;
  ByteCode?: string;
  BinFile?: string;
  BaseNamespace?: string;
  CQSNamespace?: string;
  DTONamespace?: string;
  ServiceNamespace?: string;
  CodeGenLanguage?: string;
  BaseOutputPath?: string;
}

interface Generator {
  ABIConfigurations: GeneratorConfig[];
}

export function createConfig(abiDir: string, baseName: string, outputPath: string, world?: boolean): Generator {
  const generatorConfigTemplate: GeneratorConfig = {
    ContractName: undefined,
    ABI: undefined,
    ABIFile: undefined,
    ByteCode: undefined,
    BinFile: undefined,
    BaseNamespace: baseName,
    CQSNamespace: undefined,
    DTONamespace: undefined,
    ServiceNamespace: undefined,
    CodeGenLanguage: "CSharp",
    BaseOutputPath: outputPath,
  };

  const generatedConfig: Generator = { ABIConfigurations: [] };

  if (world) {
    generatedConfig.ABIConfigurations.push({
      ...generatorConfigTemplate,
      ContractName: "IWorld",
      ABIFile: "IWorld.abi",
    });
  } else {
    const extension = ".abi";
    const files = getFilesWithExtension(abiDir, extension);
    console.log(files);

    files.forEach((file) => {
      if (!file) return;
      const config = { ...generatorConfigTemplate };
      const contractName = file.split("/").pop()?.split(".").shift();
      if (!contractName) return;
      generatedConfig.ABIConfigurations.push({
        ...config,
        ContractName: contractName,
        ABIFile: `${contractName}.abi`,
      });
    });
  }

  console.log(generatedConfig);

  return generatedConfig;
}

// Helper function to get all files with the specified extension recursively
function getFilesWithExtension(dir: string, ext: string, files: string[] = []) {
  fs.readdirSync(dir).forEach((file) => {
    const filePath = path.join(dir, file);

    if (fs.statSync(filePath).isDirectory()) {
      getFilesWithExtension(filePath, ext, files);
    } else if (path.extname(file) === ext) {
      files.push(filePath);
    }
  });
  return files;
}

function copyAndRenameFile(srcPath: string, destDir: string, newExtension: string): void {
  fs.readFile(srcPath, (readErr, data) => {
    if (readErr) {
      console.error(readErr);
      return;
    }

    // Create the destination directory if it doesn't exist
    fs.mkdir(destDir, { recursive: true }, (mkdirErr) => {
      if (mkdirErr) {
        console.error(mkdirErr);
        return;
      }

      // Get the new file name with the new extension
      const fileName = srcPath.endsWith(".abi.json")
        ? path.basename(srcPath, ".abi.json") + newExtension
        : path.basename(srcPath, path.extname(srcPath)) + newExtension;
      const destPath = path.join(destDir, fileName);

      // Write the file to the new path with the new extension
      fs.writeFile(destPath, data, (writeErr) => {
        if (writeErr) {
          console.error(writeErr);
          return;
        }
        console.log("Successfully copied and renamed file", srcPath, "to", destPath);
      });
    });
  });
}

function main() {
  const args = process.argv.slice(2);
  const outputPath = args[0] ?? "../client/Assets/Scripts";
  const abiDir = "./abi";
  fs.mkdir(abiDir, { recursive: true }, (mkdirErr) => {
    if (mkdirErr) {
      console.error(mkdirErr);
      return;
    }
  });
  const abis = "./out/IWorld.sol/IWorld.abi.json";
  copyAndRenameFile(abis, abiDir, ".abi");
  const baseName = "";
  const generatedConfig = createConfig(abiDir, baseName, outputPath, true);

  fs.writeFile(`Nethereum.Generator.json`, JSON.stringify(generatedConfig, null, 2), (err) => {
    console.error(err);
  });

  exec("dotnet tool run Nethereum.Generator.Console generate from-project", (err, stdout, stderr) => {
    if (err) {
      console.error(err);
      return;
    }
    console.log(stdout);
  });
}

main();
