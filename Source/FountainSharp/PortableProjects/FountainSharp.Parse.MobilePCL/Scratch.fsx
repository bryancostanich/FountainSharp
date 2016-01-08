

//let isLetter testString:string =
//  testString.ToCharArray() >> (Seq.forall ( fun c -> System.Char.IsLetter c || c = '_' || c = '-' ))


let isLetter2 testString:seq<string> = Seq.forall ( fun c -> (System.Char.IsLetter c || c = '_' || c = '-') ) testString

// takes a `seq<char>` 
let allLetter1 coll = coll |> Seq.forall (fun letter -> System.Char.IsLetter letter || letter = '_' || letter = '-')
// takes a `string`
let allLetter2 (coll:string) = (coll.ToCharArray()) |> Seq.forall (fun letter -> System.Char.IsLetter letter || letter = '_' || letter = '-')



let testString = "foo_eey"

let goo = (testString.ToCharArray() |> isLetter)

printfn "IsLetter: %A" (testString.ToCharArray() |> isLetter)
let isL = goo


let keyValue = ("key","value")
let testSequence = [keyValue; keyValue; keyValue;]


let isGoodPart1 sequence = Seq.forall (isLetter) sequence

//Seq.forall( fst >> 




let isGood = testSequence |> Seq.forall (fst >> Seq.forall (fun c -> System.Char.IsLetter c || c = '_' || c = '-'))

let isGood = testSequence |> Seq.forall (fst >> allLetter2)

let isGood = testSequence |> Seq.forall (fst >> fun (coll:string) -> (coll.ToCharArray()) |> Seq.forall (fun letter -> System.Char.IsLetter letter || letter = '_' || letter = '-') )


let allKeysValid kvs = 
  kvs |> Seq.forall (fst >> Seq.forall (fun c -> System.Char.IsLetter c || c = '_' || c = '-'))

let allKeysValid2 kvs = 
  kvs |> Seq.forall (fst >> fun (coll:string) -> (coll.ToCharArray()) |> Seq.forall (fun c -> System.Char.IsLetter c || c = '_' || c = '-') )


let isKey (kv:string []) = //as in [|"key";"value"|]
  // if the key isn't comprised of 
  if kv.Length >= 1 && not (Seq.forall System.Char.IsLetter (kv.[0].ToCharArray())) then None
  else None