module ResourceUtils
 
open System.Reflection
open System.Text

let enumerateResources =
  let currentAssembly = Assembly.GetCallingAssembly()
  let resourceNames = List.ofArray(currentAssembly.GetManifestResourceNames())
  resourceNames
    |> List.iter (fun r -> printfn "%A" r)
  resourceNames

let openResourceStream path =
  let currentAssembly = System.Reflection.Assembly.GetCallingAssembly()
  let stream = currentAssembly.GetManifestResourceStream(path)
  stream

let readFromResource path =
  use stream = openResourceStream (path)
  use reader = new System.IO.StreamReader(stream)
  let builder = new StringBuilder()

  let rec read() =
    let line = reader.ReadLine()
    if line <> null then
      builder.AppendLine(line) |> ignore
      read()
      
  read()
  builder.ToString()
