using System.Text;

string s = "a b bc d";
StringBuilder stringBuilder = new(s);
stringBuilder.Append(stringBuilder.Replace(" ", "_"));
System.Console.WriteLine();
