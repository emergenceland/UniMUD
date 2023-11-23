import fs from "fs";
import path from "path";

export function copyAndRenameFileExtension(
  srcPath: string,
  destDir: string,
  targetExtension: string,
  newExtension: string
): void {
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
      // const fileName = srcPath.endsWith(".abi.json")
      const fileName = srcPath.endsWith(targetExtension)
        ? path.basename(srcPath, targetExtension) + newExtension
        : path.basename(srcPath, path.extname(srcPath)) + newExtension;
      const destPath = path.join(destDir, fileName);

      // Write the file to the new path with the new extension
      fs.writeFile(destPath, data, (writeErr) => {
        if (writeErr) {
          console.error(writeErr);
          return;
        }
      });
    });
  });
}

export function getFilesWithExtension(dir: string, ext: string, files: string[] = []) {
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
