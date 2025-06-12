// MIT License
// 
// Copyright (c) 2025-2025 Hexagon Software LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using FluentAssertions;
using ThoughtSharp.Runtime.Codecs;

namespace Tests;

[TestClass]
public class TokenPacking
{
  int BitCountPerCharacter;
  PackTokenCodec Codec = null!;
  string ValidCharacters = null!;
  int Width;

  [TestInitialize]
  public void SetUp()
  {
    Width = Any.Int(16, 32);
    ValidCharacters = Any.ASCIIString(Any.Int(2, 8));
    Codec = new(ValidCharacters, Width);
    BitCountPerCharacter = (int) Math.Ceiling(Math.Log2(ValidCharacters.Length));
  }

  [TestMethod]
  public void PacksStringsThatFitInTokenWidth()
  {
    var Content = GivenAnyValidContentOfLength(Any.Int(0, Width));

    var Buffer = WhenPackBuffer(Content);

    ThenContentCharactersWerePacked(Content, Buffer, Content.Length);
  }

  [TestMethod]
  public void TruncatesStringsThatDoNotFitInTokenWidth()
  {
    var Content = GivenAnyValidContentOfLength(Width + 1);

    var Buffer = WhenPackBuffer(Content);

    ThenContentCharactersWerePacked(Content, Buffer, Width);
  }

  float[] WhenPackBuffer(string Content)
  {
    var Buffer = new float[Codec.Length];
    Codec.EncodeTo(Content, Buffer);
    return Buffer;
  }

  void ThenContentCharactersWerePacked(string Content, float[] Buffer, int PackedLength)
  {
    foreach (var I in Enumerable.Range(0, PackedLength))
      AssertChar(Buffer[(I * BitCountPerCharacter)..((I + 1) * BitCountPerCharacter)], Content[I]);
  }

  void AssertChar(Span<float> Actual, char Expected)
  {
    var Index = 1 + ValidCharacters.IndexOf(Expected);
    for (var I = 0; I < Actual.Length; ++I)
      Actual[I].Should().Be(((Index >> I) & 1) == 1 ? 1f : -1f);
  }

  string GivenAnyValidContentOfLength(int Length)
  {
    var Content = "";
    foreach (var _ in Enumerable.Range(0, Length))
      Content += Any.Of([..ValidCharacters]);

    return Content;
  }
}