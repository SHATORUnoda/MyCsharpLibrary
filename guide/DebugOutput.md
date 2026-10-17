# DebugOutput クラス 解説書

`DebugOutput` は、標準出力とファイル出力を同時に行うための `TextWriter` 実装です。

競技プログラミングでは、実行結果を画面に表示しつつ、ログや出力確認用のファイルにも残したい場面があります。

このクラスを使うと、

```csharp
Console.WriteLine("hello");
```

のような出力をそのままコンソールに表示しながら、`out.txt` にも保存できます。

---

# 目次

| メソッド | 概要 | 計算量 |
| --- | --- | --- |
| [`DebugOutput`](#debugoutput) | コンソール出力とファイル出力を同時に管理する | `O(1)` |
| [`Write`](#write) | 文字や文字列を書き込む | `O(1)` |
| [`Flush`](#flush) | バッファを強制的に出力する | `O(1)` |
| [`Init`](#init) | 標準出力をこのラッパーに差し替える | `O(1)` |

---

# 概要

`DebugOutput` は `TextWriter` を継承しており、内部で次の2つを持っています。

```csharp
public readonly TextWriter console;
public readonly TextWriter file;
```

- `console` : 通常の画面出力
- `file` : ファイル書き込み

`Write` が呼ばれるたびに、両方に出力が送られます。

---

# DebugOutput

```csharp
public DebugOutput(TextWriter console, TextWriter file)
```

## 意味

画面用の出力先とファイル用の出力先をまとめて保持するオブジェクトを作成します。

## 例

```csharp
var file = new StreamWriter("../out.txt", false)
{
    AutoFlush = true
};

var writer = new DebugOutput(Console.Out, file);
writer.WriteLine("test");
```

この場合、

- コンソールにも表示される
- `out.txt` にも保存される

という挙動になります。

---

# Write

```csharp
public override void Write(char value)
public override void Write(string value)
```

## 意味

文字または文字列を、コンソールとファイルの両方に書き込みます。

```csharp
writer.Write("abc");
writer.Write('x');
```

のように使います。

## 例

```csharp
var file = new StreamWriter("../out.txt", false)
{
    AutoFlush = true
};

var writer = new DebugOutput(Console.Out, file);
writer.Write("A");
writer.Write("BC");
```

出力は画面にもファイルにも同じ内容が入ります。

---

# Flush

```csharp
public override void Flush()
```

## 意味

バッファに残っている内容を強制的に書き出します。

`StreamWriter` では、`AutoFlush = true` を使うこともありますが、このメソッドを手動で呼ぶことで即座に出力可能です。

## 例

```csharp
writer.Write("途中の出力");
writer.Flush();
```

---

# Init

```csharp
public static void Init()
```

## 意味

`Console.SetOut` を使って、今後の標準出力を `DebugOutput` に差し替えます。

```csharp
DebugOutput.Init();
Console.WriteLine("これはファイルにも残る");
```

と書くことで、通常の `Console.WriteLine` でも、自動的に `out.txt` に書き出されます。

## 例

```csharp
DebugOutput.Init();
Console.WriteLine("テスト出力");
```

`../out.txt` に追記されるようになります。

---

# 使いどころ

以下のような場合に便利です。

* デバッグ時に内容を確認したい
* 実行ログを残したい
* 画面表示とファイル保存を同時にしたい
* 競技プログラミングの検証ログを残したい

---

# 計算量

1回の書き込みに対して、

* 時間計算量: `O(1)`
* 空間計算量: `O(1)`

です。

ただし、実際にはファイルへの書き込みのコストがかかるため、非常に大量の出力を連続で行う場合は注意が必要です。

---

# まとめ

`DebugOutput` は、

* 画面表示
* ファイル保存

を同時に行いたいときに便利なクラスです。

特にデバッグ用途や検証用途では非常に有用です。

```csharp
DebugOutput.Init();
Console.WriteLine("ログを残したい");
```

と書くだけで、動作確認の記録を残しやすくなります。
