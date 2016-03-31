module Byteville.Crawler.Helpers

let md5 (text : string) : string =
    let data = System.Text.Encoding.UTF8.GetBytes(text)
    use md5 = System.Security.Cryptography.MD5.Create()
    (System.Text.StringBuilder(), md5.ComputeHash(data))
    ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
    |> string

let (|StartsWith|_|) needle (haystack : string) = if haystack.StartsWith(needle) then Some() else None

