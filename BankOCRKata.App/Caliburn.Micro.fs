module Caliburn.Micro

open Caliburn.Micro
open System
open System.Linq.Expressions
open Microsoft.FSharp.Linq.QuotationEvaluation
open Microsoft.FSharp.Quotations

type PropertyChangedBase with
    member self.NotifyOfPropertyChange (expr: Expr<'a>) =
        let body = expr.ToLinqExpression()
        let func = Expression.Lambda<Func<'a>>(body)
        self.NotifyOfPropertyChange func
