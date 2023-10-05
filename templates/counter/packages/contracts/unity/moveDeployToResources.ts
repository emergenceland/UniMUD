import * as fs from "fs";
import * as path from "path";

export function moveToResources(inputDir: string, resourcesDir: string) {
  const inputFile = path.join(inputDir, "worlds.json");
  const outputFile = path.join(resourcesDir, "worlds.json");

  fs.copyFile(inputFile, outputFile, (err) => {
    if (err) {
      console.error("Error while copying worlds.json:", err);
    } else {
      console.log("worlds.json copied successfully");
    }
  });
}

function main() {
  const args = process.argv.slice(2);
  const outDir = args[0] ?? "../client/Assets/Resources";
  const inDir = args[1] ?? "./";
  moveToResources(inDir, outDir);
}

main();
