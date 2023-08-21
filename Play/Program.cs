Task.Run(
    () =>
    {
        try
        {
            throw new Exception("test");

        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
        }
    });
await Task.Delay(10000);