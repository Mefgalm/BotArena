//   |> List.fold (fun (state, fieldList) -> (0,0)) (0, 0) 
// return an integer exit code
module Program

open System
open System.Collections.Generic
open System.Collections.Generic
open System.Data.SqlTypes
open System.Reflection
open System.Linq
open System.Reflection
open System.Threading.Tasks
open BotArena.Types
open BotArena.CSharp.Types
open BotArena.CSharp.Attributes
open BotArena.CSharp.Interfaces
open BotArena.BlottoView
open BotArena
open Newtonsoft.Json

let cfField (field : CField) =
    { position = field.Position
      quantity = field.Quantity }

let fcField (field : Field) = CField(field.position, field.quantity)
let fcMatchBot (mp : MatchBot) = CMatchBot(BotId = mp.botId, Fields = (mp.fields |> List.map (fcField)))
let fcMatchResult (mr : MatchResult) =
    CMatchResult(OffenceBot = fcMatchBot mr.offenseBot, DefenceBot = fcMatchBot mr.defenceBot, Winner = mr.winnerBotId)

let createWrapper (ti : TypeInfo) =
    let constructor = ti.GetConstructor(Array.Empty<Type>())
    let obj = constructor.Invoke(Array.Empty<Object>())
    let methodInfo = ti.GetMethod("Invoke")
    let customAttribute = methodInfo.GetCustomAttribute<BotNameAttribute>()
    
    if customAttribute = null then raise (Exception "Attribute is missing")
    
    let wrapper (botId: string) (fieldCount : int) (tankCount : int) (results : MatchResult list) =
        methodInfo.Invoke(obj, 
                          [| botId
                             fieldCount
                             tankCount
                             results |> List.map (fcMatchResult) |]) :?> IEnumerable<CField>
        |> List.ofSeq
        |> List.map (cfField)
    
    { id = Guid.NewGuid().ToString()
      name = customAttribute.Name
      invoke = wrapper }

let createBots =
    List.ofSeq 
        (Assembly.GetEntryAssembly().GetReferencedAssemblies().Select(fun x -> Assembly.Load(x))
                 .SelectMany(fun x -> x.DefinedTypes))
    |> List.filter (fun x -> x.GetInterfaces().Any(fun y -> y = typedefof<IBlottoMethod>))
    |> List.map (createWrapper)

let skipIndex index list = (list |> List.skip (index + 1)) |> List.append (list |> List.take (index))

let rec shuffle list =
    match list with
    | [] -> Seq.empty
    | [ x ] -> seq { yield [ x ] }
    | _ -> 
        seq { 
            for i in 0..list.Length - 1 do
                yield! (shuffle (skipIndex i list)) |> Seq.map (fun x -> list.[i] :: x)
        }

let initBlottoGame fieldCount tankCount bots =
    let matches = 
        bots
        |> shuffle
        |> List.ofSeq
        |> List.map(fun x -> x 
                            |> List.map(fun y -> x 
                                                |> List.filter(fun p -> p.id <> y.id) 
                                                |> List.map(fun z -> (y, z))) 
                            |> List.collect(fun y -> y))
        |> List.map(fun x -> x |> List.map(fun (f,s) -> { offenseBot = f; defenceBot = s}))
        |> List.map(fun x -> { matches = x; results = [] })
        
    { fieldCount = fieldCount
      tankCount = tankCount
      matches = matches
      completedMatches = []
      processingMatch = {matches = []; results = []}
      bots = bots }

let isFieldCountValid fieldList fieldCount =
    if fieldList |> List.length <> fieldCount then false
    else 
        fieldList
        |> List.map (fun x -> x.position)
        |> List.sort
        |> List.zip [ 0..fieldCount - 1 ]
        |> List.forall (fun (x, y) -> x = y)

let checkFieldCount fieldCount fieldList =
    if isFieldCountValid fieldList fieldCount then fieldList
    else 
        [ 0..fieldCount - 1 ]
        |> List.map (fun i -> 
               { position = i
                 quantity = -1 })

let checkTankCount tankCount fieldList =
    if fieldList |> List.sumBy (fun x -> x.quantity) <= tankCount then fieldList
    else 
        [ 0..(fieldList |> List.length) - 1 ]
        |> List.map (fun i -> 
               { position = i
                 quantity = -1 })

let uncarry f x =
    match x with
    | (x1, x2) -> f x1 x2

let compareTwoFieldTankCount f s =
    if f > s then (1, 0)
    else (0, 1)

let sumStates initState applicator =
    match initState with
    | (fs1, ss1) -> 
        match applicator with
        | (fs2, ss2) -> (fs1 + fs2, ss1 + ss2)

let calculateMatchResult firstBotInit secondBotInit fieldCount tankCount =
    let sortAndMapFeildList fieldList =
        fieldList
        |> List.sortBy (fun x -> x.position)
        |> List.map (fun x -> x.quantity)
    
    let firstBotResult =
        firstBotInit.bot.invoke firstBotInit.bot.id fieldCount tankCount firstBotInit.marchResults
        |> checkFieldCount fieldCount
        |> checkTankCount tankCount
    
    let secondBotResult =
        secondBotInit.bot.invoke firstBotInit.bot.id fieldCount tankCount secondBotInit.marchResults
        |> checkFieldCount fieldCount
        |> checkTankCount tankCount
    
    let fieldsResult =
        secondBotResult 
        |> sortAndMapFeildList
        |> List.zip (firstBotResult |> sortAndMapFeildList)
        |> List.fold (fun state fieldList -> 
               fieldList
               |> uncarry compareTwoFieldTankCount
               |> sumStates state) (0, 0)
    
    { offenseBot =
          { botId = firstBotInit.bot.id
            fields = firstBotResult }
      defenceBot =
          { botId = secondBotInit.bot.id
            fields = secondBotResult }
      winnerBotId =
          fieldsResult
          |> uncarry (fun f s -> 
                 if f > s then firstBotInit.bot.id
                 else secondBotInit.bot.id) }

let iteration blottoGame =
    let getBotResulsts (results : MatchResult list) (botId : string) =
        results |> List.filter (fun x -> x.offenseBot.botId = botId || x.defenceBot.botId = botId)
    
    match blottoGame.matches with 
    | [] -> { fieldCount = blottoGame.fieldCount
              tankCount = blottoGame.tankCount
              matches = []
              completedMatches = blottoGame.completedMatches
              processingMatch = { matches = []; results = [] }
              bots = blottoGame.bots }
    | matchWithResults::xs ->
        match matchWithResults.matches with 
        | [] -> { fieldCount = blottoGame.fieldCount
                  tankCount = blottoGame.tankCount
                  matches = xs
                  completedMatches = blottoGame.completedMatches
                  processingMatch = { matches = []; results = [] }
                  bots = blottoGame.bots }
        | currentMatch::tailMatches ->
            let firstBotResults = getBotResulsts matchWithResults.results currentMatch.offenseBot.id
            let secondBotResults = getBotResulsts matchWithResults.results currentMatch.defenceBot.id
            
            let firstBotInit =
                { bot = currentMatch.offenseBot
                  marchResults = firstBotResults }
            
            let secondBotInit =
                { bot = currentMatch.defenceBot
                  marchResults = secondBotResults }
            
            let matchResult = calculateMatchResult firstBotInit secondBotInit blottoGame.fieldCount blottoGame.tankCount

            let newMatches = {matches = tailMatches; results = matchResult::matchWithResults.results } :: xs
            let newProcessingMatch = {matches = currentMatch :: blottoGame.processingMatch.matches; results = matchResult::matchWithResults.results} 
        
            match tailMatches with 
            | [] -> { blottoGame with matches = newMatches
                                      completedMatches = newProcessingMatch :: blottoGame.completedMatches
                                      processingMatch = {matches = []; results = []} }
            | ts -> { blottoGame with matches = newMatches
                                      completedMatches = blottoGame.completedMatches
                                      processingMatch = newProcessingMatch }
            
[<EntryPoint>]
let main argv =
    let mutable blottoGame = initBlottoGame 3 40 createBots
    
    while blottoGame.matches |> List.exists(fun x -> true) do 
        blottoGame <- blottoGame |> iteration
    
    printfn "%s" (stringBlottoView blottoGame)
        
    
    printfn "%s" (JsonConvert.SerializeObject(blottoGame, Formatting.Indented))
    
    0
