using OpenCvSharp;
using CV2.PDF;
using CV2.CV;




string pdfFilePath = @"C:\Users\syz\Desktop\计算机软件英汉双向词典-1554页 - 副本.pdf";
string pdfImagesPath = @"C:\Users\syz\Desktop\PDFImages";
FileInfo pdfFileInfo = new FileInfo(pdfFilePath);
DirectoryInfo pdfImagesDirectoryInfo = new DirectoryInfo(pdfImagesPath);
PDF.ConvertPDFToImage(pdfFileInfo, pdfImagesDirectoryInfo);

string slicedpicturesFilePath = @"C:\Users\syz\Desktop\Slicedpictures";
DirectoryInfo slicedpicturesDirectoryInfo = new DirectoryInfo(slicedpicturesFilePath);

int index = 0;
foreach (var pdfImage in pdfImagesDirectoryInfo.GetFiles())
{
    Mat image = Cv2.ImRead(pdfImage.FullName);
    if (image.Empty())
    {
        Console.WriteLine(pdfImage.FullName + ":\tEmpty");
        continue;
    }
    string ratio = "1:1";

    var images = image.VerticalLineSlicesWithRatio(ratio);

    foreach (var kvp in images)
    {
        Mat tImage = new Mat();

        Cv2.Resize(kvp.Value, tImage, new Size(350, 1000));
        Cv2.ImShow("Image", tImage);  // Show the cropped image
        string outImage = slicedpicturesDirectoryInfo.FullName + "\\" + index + ".png";
        Cv2.ImWrite(outImage, kvp.Value);
        Console.WriteLine("保存图片" + outImage);
        index++;

        Cv2.WaitKey(0);  // Wait for a key press to close the image window
    }
}







