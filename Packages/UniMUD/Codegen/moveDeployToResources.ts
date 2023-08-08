import * as fs from "fs";
import * as path from "path";

export function moveToResources(inputDir: string, resourcesDir: string) {
  const inputFile = path.join(inputDir, "latest.json");
  const outputFile = path.join(resourcesDir, "latest.json");

  fs.copyFile(inputFile, outputFile, (err) => {
    if (err) {
      console.error("Error while copying latest.json:", err);
    } else {
      console.log("latest.json copied successfully");
    }
  });
}

function main() {
  const args = process.argv.slice(2);
  const outDir = args[0] ?? "../client/Assets/Resources";
  const inDir = args[1] ?? "./deploys/31337";
  moveToResources(inDir, outDir);
}

main();
