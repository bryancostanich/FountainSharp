module ResourceUtils
 
open System.Reflection

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

let resourceToString path =
  use stream = openResourceStream (path)
  use reader = new System.IO.StreamReader(stream)
  let resourceString = reader.ReadToEnd()
  resourceString