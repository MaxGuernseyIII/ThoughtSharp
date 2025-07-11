﻿// MIT License
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

using ThoughtSharp.Runtime;

namespace ThoughtSharp.Scenarios;

public interface MindPlace
{
  Brain MakeNewBrain();
  object MakeNewMind(Brain Brain);
  void LoadSavedBrain(Brain ToLoad);
  void SaveBrain(Brain ToSave);
}

public abstract class MindPlace<TMind, TBrain> : MindPlace
  where TMind : Mind<TMind>
  where TBrain : Brain
{
  Brain MindPlace.MakeNewBrain()
  {
    return MakeNewBrain();
  }

  object MindPlace.MakeNewMind(Brain Brain)
  {
    return TMind.Create(Brain);
  }

  void MindPlace.LoadSavedBrain(Brain ToLoad)
  {
    LoadSavedBrain((TBrain) ToLoad);
  }

  void MindPlace.SaveBrain(Brain ToSave)
  {
    SaveBrain((TBrain) ToSave);
  }

  public abstract TBrain MakeNewBrain();
  public abstract void LoadSavedBrain(TBrain ToLoad);
  public abstract void SaveBrain(TBrain ToSave);
}