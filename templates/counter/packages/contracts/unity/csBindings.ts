import { exec } from "child_process";
import fs from "fs";
import { GeneratorConfig, ABIGenerator } from "./types";
import { copyAndRenameFileExtension, getFilesWithExtension } from "./utils";

export function createConfig(abiDir: string, baseName: string, outputPath: string, world?: boolean): ABIGenerator {
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

  const generatedConfig: ABIGenerator = { ABIConfigurations: [] };

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
  copyAndRenameFileExtension(abis, abiDir, ".abi.json", ".abi");
  const baseName = "";
  const generatedConfig = createConfig(abiDir, baseName, outputPath, true);

  fs.writeFile(`Nethereum.Generator.json`, JSON.stringify(generatedConfig, null, 2), (err) => {
    console.error(err);
  });

  exec("dotnet tool run Nethereum.Generator.Console generate from-project", (err, stdout, stderr) => {
    if (err) {
      console.error(err);
      console.error(stderr);
      return;
    }
    console.log(stdout);
  });
}

main();
