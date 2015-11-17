﻿module CGraph
open QuickGraph


type CgState = 
    {States: int array; 
     Accept: Set<int>; 
     Topo: Topology.State}

type T = 
    {Start: CgState;
     End: CgState;
     Graph: AdjacencyGraph<CgState, TaggedEdge<CgState, unit>>}

/// Make a shallow copy of the graph. Does not clone node values.
val copyGraph: T -> T

/// Make a shallow copy of the graph, and reverses all edges
val copyReverseGraph: T -> T

/// Constructs a new, product automaton from the topology and a collection 
/// of DFAs for path expressions
val buildFromAutomata: Topology.T -> Regex.Automaton array -> T

/// Constructs a new, product automaton from the topology and a 
/// collection of regular expression automata for different route preferences.
/// All paths through the product graph a valid topology paths that satisfy
/// one or more of the regular path constraints.
val buildFromRegex: Topology.T -> Regex.REBuilder -> Regex.T list -> T

/// Returns the set of reachable preferences
val inline preferences: T -> Set<int>

/// Returns a copy of the graph, restricted to nodes for a given preference
val restrict: T -> int -> T

/// Convert the constraint graph to the DOT format for visualization
val toDot: T -> string
   
