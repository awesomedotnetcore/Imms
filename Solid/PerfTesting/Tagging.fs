﻿namespace Benchmarks
open System

open System.Linq
open System.Collections.Generic

//These objects are used to tag and log the parameters and results of benchmarks.
open Solid

module Cache = 
    type MutableDict<'k, 'v> = Dictionary<'k, 'v>
    let  dict = Dictionary<obj,obj>()
    let inline MemoizeNoArgs<'result> (owner:obj) member_name func  = 
        if dict.ContainsKey(owner, member_name) then
            let res = dict.[(owner, member_name)]
            res :?> 'result
        else
            
            let res = func()
            dict.[(owner, member_name)] <- res
            res

type DataSource = 
    | Invalid
    | Array of int
    | FlexibleList of int
    | FSharpList of int
    | Range of int
        member x.Generate : int seq = 
            Cache.MemoizeNoArgs x "Generate" 
                (fun () -> match x with
                            | Array size -> Array.create size 0 :>_
                            | FlexibleList size-> FlexibleList.Empty.AddLastRange({0 .. size}) :>_
                            | FSharpList size-> [0 .. size] :>_
                            | Range size-> {0 .. size}
                            | Invalid -> Array.empty :>_)
        member x.Size = 
            match x with
            | Invalid -> 0
            | Array n| FlexibleList n | FSharpList n| Range n -> n
            

type Meta= 
| Initial_size of int
| Iterations of int
| Data_size of int
| Light_overhead
| Heavy_overhead
| Data_kind
| DataSource of string
| Ad_hoc_implementation
| Randomized
| Full_scope
| With_retention
| Timed_out
with override x.ToString() = sprintf "%A" x

type MetaList = 
| MetaList of Meta list
    member x.List = match x with | MetaList l -> l
    override x.ToString() =
        let str = String.Join(", ", x.List)
        str
    member x.On f = MetaList(f x.List)


type Tag = 
    {
        mutable Kind : string
        mutable Test : string
        mutable Target : string
        [<DefaultValue>]
        mutable Time : float
        mutable Metadata : MetaList
    }
    
