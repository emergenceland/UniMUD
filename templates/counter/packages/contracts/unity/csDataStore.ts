import mudConfig from "../mud.config";
import { SchemaAbiType } from "@latticexyz/schema-type";
import { TableField, schemaTypesToCSTypeStrings } from "./types";
import { writeFileSync, mkdirSync } from "fs";
import { exec } from "child_process";
import { renderFile } from "ejs";

export function createTableDefinition(
  filePath: string,
  mudConfig: any,
  tableName: string,
  keySchema: { [key: string]: SchemaAbiType },
  valueSchema: { [key: string]: SchemaAbiType }
) {
  const fields: TableField[] = [];

  for (const key in keySchema) {
    const keyType = keySchema[key];
    if (!keyType) throw new Error(`[${tableName}]: Unknown type for field ${key}`);
    fields.push({ key: key[0].toUpperCase() + key.slice(1), type: schemaTypesToCSTypeStrings[keyType] });
  }

  for (const key in valueSchema) {
    const valueType = valueSchema[key];
    if (!valueType) throw new Error(`[${tableName}]: Unknown type for field ${key}`);
    fields.push({ key: key[0].toUpperCase() + key.slice(1), type: schemaTypesToCSTypeStrings[valueType] });
  }

  renderFile(
    "./unity/templates/DefinitionTemplate.ejs",
    {
      namespace: "DefaultNamespace",
      tableClassName: tableName + "Table",
      tableName,
      fields,
    },
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

  try {
    mkdirSync(outputPath, { recursive: true });
    console.log("Directory created successfully.");
  } catch (error) {
    if (error instanceof Error) console.error("Error creating directory:", error.message);
  }

  const tables = mudConfig.tables;
  Object.entries(tables).forEach(([tableName, { keySchema, valueSchema }]) => {
    const filePath = `${outputPath}/${tableName + "Table"}.cs`;
    createTableDefinition(filePath, mudConfig, tableName, keySchema, valueSchema);
  });

  // formatting
  exec(`dotnet tool run dotnet-csharpier "${outputPath}"`, (err, stdout, stderr) => {
    if (err) {
      console.error(err);
      return;
    }
    console.log(stdout);
  });
}

main();
