[assembly:LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AwsDotnetCsharp;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using System.Net;
using System.Text.RegularExpressions;

public class Handler
{

    public async Task CreateTable(string fileName) {
      byte[] byteArray = File.ReadAllBytes(fileName);
      MemoryStream stream = new MemoryStream();
      stream.Write(byteArray, 0, (int)byteArray.Length);

      var textReplaces = new Dictionary<string, string>();
      textReplaces.Add("{{var1}}", "Example Name");
      textReplaces.Add("{{var2}}", "External asset workflow");
      textReplaces.Add("{{var3}}", "302929393");
      textReplaces.Add("{{var4}}", "LGIM/Client user");
      textReplaces.Add("{{var5}}", "+972-5056000000");
      textReplaces.Add("{{var6}}", "example@example.com");
      textReplaces.Add("{{var7}}", "Active Client page – External assets");
      textReplaces.Add("{{var8}}", "Information button text");
      textReplaces.Add("{{var10}}", "03 Jan, 2022");
      textReplaces.Add("{{var11}}", "10,000");
      textReplaces.Add("{{var12}}", "3,000");
      textReplaces.Add("{{var13}}", "7,000");

      using (WordprocessingDocument doc = WordprocessingDocument.Open(stream, true)) {
        // Replacing
        string docText = null;
        using (StreamReader sr = new StreamReader(doc.MainDocumentPart.GetStream()))
        {
            docText = sr.ReadToEnd();
        }

        foreach (KeyValuePair<string, string> entry in textReplaces)
        {
            Regex regexText = new Regex(entry.Key);
            docText = regexText.Replace(docText, entry.Value);
        }

        using (StreamWriter sw = new StreamWriter(doc.MainDocumentPart.GetStream(FileMode.Create)))
        {
            sw.Write(docText);
        }

        var tables = doc.MainDocumentPart.Document.Descendants<Table>();

        int i = 0;
        while (i < 1500) {
          var copyTable = tables.ElementAt(1).CloneNode(true);
          doc.MainDocumentPart.Document.Body.Append(new Paragraph(new Run(new Text(" "))));
          doc.MainDocumentPart.Document.Body.Append(copyTable);
          i++;
        }

        // doc.MainDocumentPart.Document.Body.Append(new Paragraph(new Run(new Text(" "))));
        foreach (Table t in tables)
        {
            var rows = t.Descendants<DocumentFormat.OpenXml.Wordprocessing.TableRow>();
            foreach (DocumentFormat.OpenXml.Wordprocessing.TableRow row in rows) {
              foreach (DocumentFormat.OpenXml.Wordprocessing.TableCell cll in row.Descendants<DocumentFormat.OpenXml.Wordprocessing.TableCell>()) {
                foreach (DocumentFormat.OpenXml.Wordprocessing.Paragraph pgraph in cll.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()) {
                  pgraph.Append(new Run(new Text("sample test 1")));
                }
                // Console.WriteLine(cll);
                // cll.Append(new Paragraph(new Run(new Text("sample test 1"))));
              }
            }
                    
            // t.Append(new TableRow(new TableCell(new Paragraph(new Run(new Text("sample test 1"))))));
            // Console.WriteLine(t);
        }

        // int i = 0;
        // while (i < 1500) {
        //   // Create an empty table.
        //   Table table = new Table();

        //   // Create a TableProperties object and specify its border information.
        //   TableProperties tblProp = new TableProperties(
        //       new TableBorders(
        //           new TopBorder() { Val = 
        //               new EnumValue<BorderValues>(BorderValues.Dashed), Size = 24 },
        //           new BottomBorder() { Val = 
        //               new EnumValue<BorderValues>(BorderValues.Dashed), Size = 24 },
        //           new LeftBorder() { Val = 
        //               new EnumValue<BorderValues>(BorderValues.Dashed), Size = 24 },
        //           new RightBorder() { Val = 
        //               new EnumValue<BorderValues>(BorderValues.Dashed), Size = 24 },
        //           new InsideHorizontalBorder() { Val = 
        //               new EnumValue<BorderValues>(BorderValues.Dashed), Size = 24 },
        //           new InsideVerticalBorder() { Val = 
        //               new EnumValue<BorderValues>(BorderValues.Dashed), Size = 24 }
        //       )
        //   );

        //   // Append the TableProperties object to the empty table.
        //   table.AppendChild<TableProperties>(tblProp);

        //   // Create a row.
        //   TableRow tr = new TableRow();

        //   // Create a cell.
        //   TableCell tc1 = new TableCell();

        //   // Specify the width property of the table cell.
        //   tc1.Append(new TableCellProperties(
        //       new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = "2400" }));

        //   // Specify the table cell content.
        //   tc1.Append(new Paragraph(new Run(new Text("some text"))));

        //   // Append the table cell to the table row.
        //   tr.Append(tc1);

        //   // Create a second table cell by copying the OuterXml value of the first table cell.
        //   TableCell tc2 = new TableCell(tc1.OuterXml);

        //   // Append the table cell to the table row.
        //   tr.Append(tc2);

        //   // Append the table row to the table.
        //   table.Append(tr);

        //   // Append the table to the document.
        //   doc.MainDocumentPart.Document.Body.Append(table);
        //   i++;
        // }
      }

     File.WriteAllBytes(@"/tmp/output.docx", stream.ToArray());

      byte[] bA = File.ReadAllBytes(@"/tmp/output.docx");
      MemoryStream s = new MemoryStream();
      s.Write(bA, 0, (int)bA.Length);

      // save to s3
      var upoloaded = await saveToS3("poc-docx-cs", s);
    }

    private async Task<bool> saveToS3(string bucketName, MemoryStream content) {
      var request = new PutObjectRequest {
        BucketName = bucketName,
        Key = "myobj.docx",
        InputStream = content
      };

      using (var s3 = new AmazonS3Client(RegionEndpoint.USEast1)) {
        var response = await s3.PutObjectAsync(request);
        if (response.HttpStatusCode == HttpStatusCode.OK) {
          return true;
        } else {
          return false;
        }
      }
    }

    public async Task<Response> DocxGenerate(Request request)
    {
        await CreateTable("doc1.docx");
        return new Response("Go Serverless v1.0! Your function executed successfully!", request);
    }
}

public class Response
{
  public string Message {get; set;}
  public Request Request {get; set;}

  public Response(string message, Request request)
  {
    Message = message;
    Request = request;
  }
}

public class Request
{
  public string Key1 {get; set;}
  public string Key2 {get; set;}
  public string Key3 {get; set;}
}
