namespace BotArena.Types

open Newtonsoft.Json
open System
open System.Collections.Generic

type Field =
    { position : int
      quantity : int }

type MatchBot =
    { botId : string
      name : string
      fields : Field list }

type MatchResult =
    { offenseBot : MatchBot
      defenceBot : MatchBot
      winnerBotId : string }

type Bot =
    { id : string
      name : string
      [<JsonIgnore>]
      invoke : string -> int -> int -> MatchResult list -> Field list }

type Match =
    { offenseBot : Bot
      defenceBot : Bot }
      
type MatchWithResults = 
   { matches: Match list
     results: MatchResult list }

type BotInit =
    { bot: Bot
      marchResults: MatchResult list }

type BlottoGame =
    { fieldCount : int
      tankCount : int
      matches : MatchWithResults list
      completedMatches : MatchWithResults list
      processingMatch : MatchWithResults 
      bots : Bot list }
      

