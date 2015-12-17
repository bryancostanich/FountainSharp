namespace FountainSharp.Parse.Tests
open System

type Exec (args) = 
   let waitForKey () =
        printf "\nPress any key to continue ..."
        System.Console.ReadKey() |> ignore
   
   let exit x = 
        waitForKey()
        x

   member x.Run()
        printf "test."
        exit 1