﻿module Topology

open QuickGraph


/// Classify topology nodes according to type
type NodeType = 
    | Start
    | End
    | Outside
    | Inside 
    | InsideOriginates

/// Type for a node in the topology. Given by its name and type
type State = 
    {Loc: string; 
     Typ: NodeType}

/// Alternative representation of the topology as a map.
/// Gives fast access to 
type NeighborMap = Map<State, Set<State>>

(* Topology as a directed graph between States *)
type T = BidirectionalGraph<State,TaggedEdge<State,unit>>


(* Build the internal and external alphabet from a topology *)
let alphabet (topo: T) : Set<State> * Set<State> = 
    let mutable ain = Set.empty 
    let mutable aout = Set.empty 
    for v in topo.Vertices do
        match v.Typ with 
        | Inside -> ain <- Set.add v ain
        | InsideOriginates -> ain <- Set.add v ain
        | Outside -> aout <- Set.add v aout
        | Start | End -> failwith "unreachable"
    (ain, aout)


(* Construct the neighbor map representation from a topology *)
let neighborMap (topo: T) : NeighborMap   = 
    let mutable nmap = Map.empty
    for v in topo.Vertices do
        let mutable adj = Set.empty 
        for e in topo.OutEdges v do 
            adj <- Set.add e.Target adj
        for e in topo.InEdges v do 
            adj <- Set.add e.Source adj
        nmap <- Map.add v adj nmap
    nmap

let isTopoNode (t: State) = 
    match t.Typ with 
    | Start | End -> false
    | Outside | Inside | InsideOriginates -> true

let isInside (t: State) = 
    match t.Typ with 
    | Inside | InsideOriginates ->  true 
    | Outside | Start | End -> false

let canOriginateTraffic (t: State) = 
    match t.Typ with 
    | InsideOriginates -> true 
    | Outside -> true
    | Inside -> false
    | Start | End -> false


(* TODO: check for duplicate topology nodes *)
(* TODO: well defined in and out (fully connected inside) *)
let isWellFormed (t: State) = 
    failwith "TODO"


(* Helper module for enumerating and constructing topology failure scenarios *)
module Failure = 

    type FailType = 
        | NodeFailure of State 
        | LinkFailure of TaggedEdge<State,unit>

    let combinations n ls = 
        let rec aux acc size set = seq {
            match size, set with 
            | n, x::xs -> 
                if n > 0 then yield! aux (x::acc) (n - 1) xs
                if n >= 0 then yield! aux acc n xs 
            | 0, [] -> yield acc 
            | _, [] -> () }
        aux [] n ls

    let allFailures n (topo: T) : seq<FailType list> = 
        let fvs = topo.Vertices |> Seq.filter isInside |> Seq.map NodeFailure
        
        let fes = 
            topo.Edges 
            |> Seq.filter (fun e -> isInside e.Source || isInside e.Target) 
            |> Seq.map LinkFailure 
        
        Seq.append fes fvs 
        |> Seq.toList 
        |> combinations n


module Example1 = 
    let topo () = 
        let g = BidirectionalGraph<State ,TaggedEdge<State,unit>>()
        let vA = {Loc="A"; Typ=InsideOriginates}
        let vX = {Loc="X"; Typ=Inside}
        let vM = {Loc="M"; Typ=Inside}
        let vN = {Loc="N"; Typ=Inside}
        let vY = {Loc="Y"; Typ=Inside}
        let vZ = {Loc="Z"; Typ=Inside}
        let vB = {Loc="B"; Typ=InsideOriginates}
        g.AddVertex vA |> ignore 
        g.AddVertex vX |> ignore 
        g.AddVertex vM |> ignore 
        g.AddVertex vN |> ignore 
        g.AddVertex vY |> ignore 
        g.AddVertex vZ |> ignore 
        g.AddVertex vB |> ignore 
        g.AddEdge (TaggedEdge(vA, vX, ())) |> ignore
        g.AddEdge (TaggedEdge(vA, vM, ())) |> ignore
        g.AddEdge (TaggedEdge(vM, vN, ())) |> ignore
        g.AddEdge (TaggedEdge(vX, vN, ())) |> ignore
        g.AddEdge (TaggedEdge(vN, vY, ())) |> ignore
        g.AddEdge (TaggedEdge(vN, vZ, ())) |> ignore
        g.AddEdge (TaggedEdge(vY, vB, ())) |> ignore
        g.AddEdge (TaggedEdge(vZ, vB, ())) |> ignore
        g


module Example2 = 
    let topo () = 
        let g = BidirectionalGraph<State, TaggedEdge<State,unit>>()
        let vA = {Loc="A"; Typ=InsideOriginates}
        let vB = {Loc="B"; Typ=InsideOriginates}
        let vC = {Loc="C"; Typ=InsideOriginates}
        let vD = {Loc="D"; Typ=InsideOriginates}
        let vX = {Loc="X"; Typ=Inside}
        let vY = {Loc="Y"; Typ=Inside}
        let vM = {Loc="M"; Typ=Inside}
        let vN = {Loc="N"; Typ=Inside}
        g.AddVertex vA |> ignore 
        g.AddVertex vB |> ignore 
        g.AddVertex vC |> ignore 
        g.AddVertex vD |> ignore 
        g.AddVertex vX |> ignore 
        g.AddVertex vY |> ignore 
        g.AddVertex vM |> ignore 
        g.AddVertex vN |> ignore 
        g.AddEdge (TaggedEdge(vA, vX, ())) |> ignore
        g.AddEdge (TaggedEdge(vX, vA, ())) |> ignore
        g.AddEdge (TaggedEdge(vB, vX, ())) |> ignore
        g.AddEdge (TaggedEdge(vX, vB, ())) |> ignore
        g.AddEdge (TaggedEdge(vC, vY, ())) |> ignore
        g.AddEdge (TaggedEdge(vY, vC, ())) |> ignore
        g.AddEdge (TaggedEdge(vD, vY, ())) |> ignore
        g.AddEdge (TaggedEdge(vY, vD, ())) |> ignore
        g.AddEdge (TaggedEdge(vX, vM, ())) |> ignore
        g.AddEdge (TaggedEdge(vM, vX, ())) |> ignore
        g.AddEdge (TaggedEdge(vX, vN, ())) |> ignore
        g.AddEdge (TaggedEdge(vN, vX, ())) |> ignore
        g.AddEdge (TaggedEdge(vY, vM, ())) |> ignore
        g.AddEdge (TaggedEdge(vM, vY, ())) |> ignore
        g.AddEdge (TaggedEdge(vY, vN, ())) |> ignore
        g.AddEdge (TaggedEdge(vN, vY, ())) |> ignore
        g


module Example3 = 
    let topo () = 
        let g = BidirectionalGraph<State, TaggedEdge<State,unit>>()
        let vA = {Loc="A"; Typ=InsideOriginates}
        let vB = {Loc="B"; Typ=InsideOriginates}
        let vC = {Loc="C"; Typ=Inside}
        let vD = {Loc="D"; Typ=Inside}
        let vE = {Loc="E"; Typ=InsideOriginates}
        let vF = {Loc="F"; Typ=InsideOriginates}
        let vG = {Loc="G"; Typ=Inside}
        let vH = {Loc="H"; Typ=Inside}
        let vX = {Loc="X"; Typ=Inside}
        let vY = {Loc="Y"; Typ=Inside}
        g.AddVertex vA |> ignore 
        g.AddVertex vB |> ignore 
        g.AddVertex vC |> ignore 
        g.AddVertex vD |> ignore 
        g.AddVertex vE |> ignore 
        g.AddVertex vF |> ignore 
        g.AddVertex vG |> ignore 
        g.AddVertex vH |> ignore 
        g.AddVertex vX |> ignore
        g.AddVertex vY |> ignore
        g.AddEdge (TaggedEdge(vA, vC, ())) |> ignore
        g.AddEdge (TaggedEdge(vC, vA, ())) |> ignore
        g.AddEdge (TaggedEdge(vA, vD, ())) |> ignore
        g.AddEdge (TaggedEdge(vD, vA, ())) |> ignore
        g.AddEdge (TaggedEdge(vB, vC, ())) |> ignore
        g.AddEdge (TaggedEdge(vC, vB, ())) |> ignore
        g.AddEdge (TaggedEdge(vB, vD, ())) |> ignore
        g.AddEdge (TaggedEdge(vD, vB, ())) |> ignore
        g.AddEdge (TaggedEdge(vE, vG, ())) |> ignore
        g.AddEdge (TaggedEdge(vG, vE, ())) |> ignore
        g.AddEdge (TaggedEdge(vE, vH, ())) |> ignore
        g.AddEdge (TaggedEdge(vH, vE, ())) |> ignore
        g.AddEdge (TaggedEdge(vF, vG, ())) |> ignore
        g.AddEdge (TaggedEdge(vG, vF, ())) |> ignore
        g.AddEdge (TaggedEdge(vF, vH, ())) |> ignore
        g.AddEdge (TaggedEdge(vH, vF, ())) |> ignore
        g.AddEdge (TaggedEdge(vC, vX, ())) |> ignore
        g.AddEdge (TaggedEdge(vX, vC, ())) |> ignore
        g.AddEdge (TaggedEdge(vC, vY, ())) |> ignore
        g.AddEdge (TaggedEdge(vY, vC, ())) |> ignore
        g.AddEdge (TaggedEdge(vD, vX, ())) |> ignore
        g.AddEdge (TaggedEdge(vX, vD, ())) |> ignore
        g.AddEdge (TaggedEdge(vD, vY, ())) |> ignore
        g.AddEdge (TaggedEdge(vY, vD, ())) |> ignore
        g.AddEdge (TaggedEdge(vG, vX, ())) |> ignore
        g.AddEdge (TaggedEdge(vX, vG, ())) |> ignore
        g.AddEdge (TaggedEdge(vG, vY, ())) |> ignore
        g.AddEdge (TaggedEdge(vY, vG, ())) |> ignore
        g.AddEdge (TaggedEdge(vH, vX, ())) |> ignore
        g.AddEdge (TaggedEdge(vX, vH, ())) |> ignore
        g.AddEdge (TaggedEdge(vH, vY, ())) |> ignore
        g.AddEdge (TaggedEdge(vY, vH, ())) |> ignore
        g