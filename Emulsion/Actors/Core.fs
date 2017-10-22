module Emulsion.Actors.Core

open Akka.Actor

open Emulsion

type CoreActor(factories : ActorFactories) as this =
    inherit ReceiveActor()

    do this.Receive<Message>(this.OnMessage)
    let mutable xmpp = Unchecked.defaultof<IActorRef>
    let mutable telegram = Unchecked.defaultof<IActorRef>

    member private this.spawn (factory : ActorFactory) name =
        factory ActorBase.Context this.Self name

    override this.PreStart() =
        printfn "Starting Core actor..."
        xmpp <- this.spawn factories.xmppFactory "xmpp"
        telegram <- this.spawn factories.telegramFactory "telegram"

    member this.OnMessage(message : Message) : unit =
        match message with
        | TelegramMessage text -> xmpp.Tell(text, this.Self)
        | XmppMessage text -> telegram.Tell(text, this.Self)

let spawn (factories : ActorFactories) (system : IActorRefFactory) (name : string) : IActorRef =
    printfn "Spawning Core..."
    let props = Props.Create<CoreActor>(factories)
    system.ActorOf(props, name)