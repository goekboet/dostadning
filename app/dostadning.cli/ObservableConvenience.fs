namespace dostadning.cli

open System
open System.Reactive.Linq

module ObservableConvenience =
    let select (f : 'a -> 'b) (o : IObservable<'a>) = Observable.Select(o, f)
    let take (n : int) (o : IObservable<'a>) = Observable.Take(o, n)
    let log lr (le : Exception -> Unit) o = Observable.Do(o, lr, le) 
    let catch (h : Exception -> IObservable<'a>) o = Observable.Catch(o, h)
    let run = Observable.Wait
    
    let cue _ = System.Reactive.Unit.Default
    let cues n = TimeSpan.FromMilliseconds >> Observable.Interval >> (select cue) >> (take n)
    
    let LogResultOrError le lr r  = 
        r()
        |> log lr le 
        |> select (fun _ -> 1)
        |> catch (fun _ -> Observable.Return 0)
    let Errorlog (e : Exception) = printfn "msg: %s trace: %s" e.Message e.StackTrace 
    let LogResult lr r = LogResultOrError Errorlog lr r

    