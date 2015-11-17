﻿module Config
open Extension.Error
open CGraph

type Match = 
    | Peer of string 
    | State of int array * string
    | PathRE of Regex.T

    override this.ToString () = 
        match this with 
        | Peer s -> "Peer=" + s
        | State(is,s) -> "Community=" + is.ToString() + "," + s
        | PathRE r -> "Regex=" + r.ToString() 

type Action = 
    | NoAction
    | SetComm of int array * string
    | SetMed of int
    | SetLP of int

    override this.ToString() = 
        match this with 
        | NoAction -> ""
        | SetComm(is,s) -> "Community<-" + is.ToString() + "," + s
        | SetMed i -> "MED<-" + i.ToString()
        | SetLP i -> "LP<-" + i.ToString()

type Actions = Action list

type Rule =
    {Import: Match;
     Export: Actions}

type T = Map<string, Rule list>

let format (config: T) = 
    let sb = System.Text.StringBuilder ()
    for kv in config do 
        sb.Append("Router ") |> ignore
        sb.Append(kv.Key) |> ignore
        for rule in kv.Value do
            sb.Append("\n  Match: ") |> ignore
            sb.Append(rule.Import.ToString()) |> ignore
            sb.Append("\n    Export: ") |> ignore
            sb.Append(rule.Export.ToString()) |> ignore
        sb.Append("\n\n") |> ignore
    sb.ToString()

let private genConfig (cg: CGraph.T) (ord: Consistency.Ordering) : T =
    let compareLocThenPref (x,i1) (y,i2) = 
        let cmp = compare i1 i2
        if cmp = 0 then
            compare x.Topo.Loc y.Topo.Loc
        else cmp

    let rec removeAdjacentLocs sorted = 
        match sorted with 
        | [] | [_] -> sorted
        | hd1::((hd2::z) as tl) ->
            let (x,i1) = hd1 
            let (y,i2) = hd2 
            if x.Topo.Loc = y.Topo.Loc then removeAdjacentLocs (hd1::z)
            else hd1 :: (removeAdjacentLocs tl)
    
    let cgRev = copyReverseGraph cg
    let neighborsIn v = 
        seq {for e in cgRev.Graph.OutEdges v do 
                if e.Target.Topo.Typ <> Topology.Start then yield e.Target}
    let neighborsOut v = 
        seq {for e in cg.Graph.OutEdges v do
                if e.Target.Topo.Typ <> Topology.Start then yield e.Target}
    let mutable config = Map.empty
    for entry in ord do 
        let mutable rules = []
        let loc = entry.Key 
        let prefs = entry.Value 
        let prefNeighborsIn = 
            prefs
            |> Seq.mapi (fun i v -> (neighborsIn v, i))
            |> Seq.map (fun (ns,i) -> Seq.map (fun n -> (n,i)) ns) 
            |> Seq.fold Seq.append Seq.empty 
            |> List.ofSeq
            |> List.sortWith compareLocThenPref
            |> removeAdjacentLocs
        let mutable lp = 99
        let mutable lastPref = None
        for v, pref in prefNeighborsIn do 
            match lastPref with 
            | Some p when pref = p -> () 
            | _ ->
                lastPref <- Some pref 
                lp <- lp + 1
            let unambiguous = 
                prefNeighborsIn 
                |> Set.ofList 
                |> Set.filter (fun (x,_) -> x.Topo.Loc = v.Topo.Loc) 
                |> Set.count 
                |> ((=) 1)
            let m = 
                if unambiguous then Peer v.Topo.Loc 
                else State (v.States, v.Topo.Loc)
            let a = 
                if lp = 100 then [] 
                else [SetLP(lp)]
            rules <- {Import = m; Export = a}::rules
        config <- Map.add loc rules config
    config

let compile (topo: Topology.T) (cg: CGraph.T) : Result<T, Consistency.CounterExample> =
    match Consistency.findOrdering cg with 
    | Ok ord -> Ok (genConfig cg ord)
    | Err(x) -> Err(x)


(* Generate templates 
let generateTemplates (config: T) (path: string) = 
    failwith "todo" *)
