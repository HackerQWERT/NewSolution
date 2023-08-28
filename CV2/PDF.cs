using MuPDFCore;

namespace CV2.PDF;






public static class PDF
{
    public static void ConvertPDFToImage(FileInfo pdfFileInfo, DirectoryInfo pdfImages)
    {
        var filePath = pdfFileInfo.FullName;

        using var pdfContext = new MuPDFContext();
        using var pdfDocument = new MuPDFDocument(pdfContext, filePath);

        for (var i = 0; i < pdfDocument.Pages.Count; i++)
        {
            string outImage = pdfImages.FullName + "\\" + i + ".png";
            pdfDocument.SaveImage(i, 4, PixelFormats.RGB, outImage, RasterOutputFileTypes.PNG);
        }

    }


}
