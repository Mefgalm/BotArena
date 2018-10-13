namespace BotArena.Types

open System
open System.Collections.Generic

type Field =
    { position : int
      quantity : int }

type MatchBot =
    { botId : string
      fields : Field list }

type MatchResult =
    { firstBot : MatchBot
      secondBot : MatchBot
      winnerBotId : string }

type Bot =
    { id : string
      name : string
      invoke : int -> int -> MatchResult list -> Field list }

type Match =
    { firstBot : Bot
      secondBot : Bot }

type BlottoGame =
    { fieldCount : int
      tankCount : int
      matches : Match list
      bots : Bot list
      results : MatchResult list }
      
type BotInit =
    { bot: Bot
      marchResults: MatchResult list }
