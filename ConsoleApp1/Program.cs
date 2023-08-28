using var pdfContext = new MuPDFContext();
using var pdfDocument = new MuPDFDocument(pdfContext, filePath);
pdfDocument.SaveImage(index - 1, 4, PixelFormats.RGB, fileInfo.FullName, RasterOutputFileTypes.PNG);
