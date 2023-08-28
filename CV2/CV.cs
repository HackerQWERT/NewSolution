using OpenCvSharp;

namespace CV2.CV;

public static class OpenCvExtensions
{

    public static Dictionary<string, Mat> VerticalLineSlicesWithRatio(this Mat image, string ratioString/*, SlicesParameters parameters*/)
    {

        var totalWidth = image.Width;
        Dictionary<string, Mat> imageDictionary = new Dictionary<string, Mat>();
        List<float> ratios = new List<float>();
        float sum = 0;
        try
        {
            var tRatios = ratioString.Split(":").Select(x => float.Parse(x)).ToList();
            sum = tRatios.Sum();
            ratios = tRatios.Select(x => x / sum).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        int startX = 0;
        for (int j = 0; j < ratios.Count; j++)
        {
            // 根据输入的比例计算切割的宽度
            var sliceWidth = (int)(totalWidth * ratios[j]);
            int endX = startX + sliceWidth;

            // 计算切割的区域
            var rect = new Rect(x: startX, y: 0, width: sliceWidth, height: image.Height); ;


            var rectMat = new Mat(image, rect);


            string outFile = @"C:\Users\syz\Desktop\Images" + Guid.NewGuid();

            imageDictionary.Add(outFile, rectMat);

            //计算前面的距离
            startX += sliceWidth;

        }
        return imageDictionary;

    }
}
