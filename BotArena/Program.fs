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

let cfField (field : CField) =
    { position = field.Position
      quantity = field.Quantity }

let fcField (field : Field) = CField(field.position, field.quantity)
let fcMatchBot (mp : MatchBot) = CMatchBot(PlayerId = mp.botId, Fields = (mp.fields |> List.map (fcField)))
let fcMatchResult (mr : MatchResult) =
    CMatchResult(FirstBot = fcMatchBot mr.firstBot, SecondBot = fcMatchBot mr.secondBot, Winner = mr.winnerBotId)

let createWrapper (ti : TypeInfo) =
    let constructor = ti.GetConstructor(Array.Empty<Type>())
    let obj = constructor.Invoke(Array.Empty<Object>())
    let methodInfo = ti.GetMethod("Invoke")
    let customAttribute = methodInfo.GetCustomAttribute<BotNameAttribute>()
    
    if customAttribute = null then raise (Exception "Attribute is missing")
    
    let wrapper (fieldCount : int) (tankCount : int) (results : MatchResult list) =
        methodInfo.Invoke(obj, 
                          [| fieldCount
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
        |> List.map (fun x -> 
               bots
               |> List.filter (fun y -> y.id <> x.id)
               |> List.map (fun z -> (x, z)))
        |> List.collect (fun x -> x)
        |> List.map (fun (f, s) -> 
               { firstBot = f
                 secondBot = s })
        |> shuffle
        |> Seq.collect (fun x -> x)
        |> List.ofSeq
    { fieldCount = fieldCount
      tankCount = tankCount
      matches = matches
      bots = bots
      results = [] }

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
        firstBotInit.bot.invoke fieldCount tankCount firstBotInit.marchResults
        |> checkFieldCount fieldCount
        |> checkTankCount tankCount
    
    let secondBotResult =
        secondBotInit.bot.invoke fieldCount tankCount secondBotInit.marchResults
        |> checkFieldCount fieldCount
        |> checkTankCount tankCount
    
    let fieldsResult =
        firstBotResult
        |> sortAndMapFeildList
        |> List.zip (secondBotResult |> sortAndMapFeildList)
        |> List.fold (fun state fieldList -> 
               fieldList
               |> uncarry compareTwoFieldTankCount
               |> sumStates state) (0, 0)
    
    { firstBot =
          { botId = firstBotInit.bot.id
            fields = firstBotResult }
      secondBot =
          { botId = secondBotInit.bot.id
            fields = secondBotResult }
      winnerBotId =
          fieldsResult
          |> uncarry (fun f s -> 
                 if f > s then firstBotInit.bot.id
                 else secondBotInit.bot.id) }

let iteration blottoGame =
    let getBotResulsts (results : MatchResult list) (botId : string) =
        results |> List.filter (fun x -> x.firstBot.botId = botId || x.secondBot.botId = botId)
    let m = blottoGame.matches.Head
    let firstBotResults = getBotResulsts blottoGame.results m.firstBot.id
    let secondBotResults = getBotResulsts blottoGame.results m.secondBot.id
    
    let firstBotInit =
        { bot = m.firstBot
          marchResults = firstBotResults }
    
    let secondBotInit =
        { bot = m.secondBot
          marchResults = secondBotResults }
    
    let matchResult = calculateMatchResult firstBotInit secondBotInit blottoGame.fieldCount blottoGame.tankCount
    { fieldCount = blottoGame.fieldCount
      tankCount = blottoGame.tankCount
      matches = blottoGame.matches.Tail
      bots = blottoGame.bots
      results = matchResult :: blottoGame.results }


[<EntryPoint>]
let main argv =
    let mutable blottoGame = initBlottoGame 3 40 createBots
    
    while blottoGame.matches |> List.exists(fun x -> true) do 
        blottoGame <- blottoGame |> iteration
    
    printfn "%s" (stringBlottoView blottoGame)
    
    0
