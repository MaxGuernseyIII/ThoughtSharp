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

namespace ThoughtSharp.Generator;

static class CognitiveAttributeNames
{
  public const string Namespace = "ThoughtSharp.Runtime.";
  public const string DataAttributeName = "CognitiveDataAttribute";
  public const string ActionsAttributeName = "CognitiveActionsAttribute";
  public const string CategoryAttributeName = "CognitiveCategoryAttribute";
  public const string FullDataAttribute = Namespace + DataAttributeName;
  public const string FullActionsAttribute = Namespace + ActionsAttributeName;
  public const string FullCategoryAttribute = Namespace + CategoryAttributeName + "`2";
  public const string FullMindAttribute = Namespace + MindAttributeName;
  public const string MindAttributeName = "MindAttribute";
  public const string UseAttributeName = "UseAttribute";
  public const string ChooseAttributeName = "ChooseAttribute";
  public const string TellAttributeName = "TellAttribute";
  public const string MakeAttributeName = "MakeAttribute";
  public const string DataCountAttributeName = "CognitiveDataCountAttribute";
  public const string DataLengthAttributeName = "CognitiveDataLengthAttribute";
  public const string DataBoundsAttributeName = "CognitiveDataBoundsAttribute";
  public const string BitwiseAttributeName = "BitwiseAttribute";
  public const string CategoricalAttributeName = "CategoricalAttribute";
}