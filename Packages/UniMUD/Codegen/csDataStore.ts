import mudConfig from "../mud.config";
import { AbiTypeToSchemaType, SchemaType } from "@latticexyz/schema-type";
import { schemaTypesToCSTypeStrings } from "./types";
import { StoreConfig } from "@latticexyz/store";
import { WorldConfig } from "@latticexyz/world";
import { writeFileSync, mkdirSync } from "fs";
import { exec, execSync } from "child_process";
import { renderFile } from "ejs";
import { basename, dirname, extname, join } from "path";

interface Field {
  key: string;
  type: string;
}

export async function createCSComponents(filePath: string, mudConfig: any, tableName: string, tableData: any) {
  const fields: Field[] = [];
  const worldNamespace = tableData.namespace;

  for (const key in tableData.schema) {
    const abiOrUserType = tableData.schema[key];
    if (abiOrUserType in AbiTypeToSchemaType) {
      const schemaType = AbiTypeToSchemaType[abiOrUserType];
      const typeString = schemaTypesToCSTypeStrings[schemaType];
      fields.push({ key, type: typeString });
    } else if (abiOrUserType in mudConfig.enums) {
      const schemaType = SchemaType.UINT8;
      fields.push({ key, type: schemaTypesToCSTypeStrings[schemaType] });
    } else {
      throw new Error(`Unknown type ${abiOrUserType} for field ${key}`);
    }
  }

  console.log("Generating UniMUD table for " + tableName);
  renderFile(
    "./unity/templates/DefinitionTemplate.ejs",
    {
      namespace: "DefaultNamespace",
      tableClassName: tableName + "Table",
      tableName,
      tableNamespace: worldNamespace,
      fields: fields,
    },
    {},
    (err, str) => {
      console.log("writeFileSync " + filePath);
      writeFileSync(filePath, str);
      if (err) throw err;
    }
  );
}

async function main() {
  // get args
  const args = process.argv.slice(2);
  const outputPath = args[0] ?? `../client/Assets/Scripts/codegen`;
  console.log(outputPath);

  // create the folder if it doesn't exist
  try {
    mkdirSync(outputPath, { recursive: true });
    console.log("Directory created successfully.");
  } catch (error) {
    if (error instanceof Error) console.error("Error creating directory:", error.message);
  }

  const tables = mudConfig.tables;
  Object.entries(tables).forEach(async ([tableName, tableData]) => {
    const filePath = `${outputPath}/${tableName + "Table"}.cs`;
    await createCSComponents(filePath, mudConfig, tableName, tableData);
  });

  exec(`dotnet tool run dotnet-csharpier "${outputPath}"`, (err, stdout, stderr) => {
    if (err) {
      console.error(err);
      return;
    }
    console.log(stdout);
  });
}

main();
