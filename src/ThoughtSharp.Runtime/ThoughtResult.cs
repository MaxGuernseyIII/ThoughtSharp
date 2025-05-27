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

using System.Runtime.ExceptionServices;

namespace ThoughtSharp.Runtime;

public class ThoughtResult<TProduct, TFeedback>(
  TProduct? Product,
  TFeedback? Feedback,
  ExceptionDispatchInfo? Exception) : ThoughtResult<TFeedback>(Feedback, Exception)
{
  public TProduct Product
  {
    get
    {
      RaiseAnyExceptions();

      return field;
    }
  } = Product!;
}

public abstract class ThoughtResult<TFeedback>(TFeedback? Feedback, ExceptionDispatchInfo? Exception)
  : ThoughtResult(Exception)
{
  public TFeedback Feedback
  {
    get
    {
      if (field is null)
        RaiseAnyExceptions();

      return field!;
    }
  } = Feedback!;
}

public abstract class ThoughtResult(ExceptionDispatchInfo? Exception)
{
  public void RaiseAnyExceptions()
  {
    Exception?.Throw();
  }

  public static WithFeedbackFactory<TFeedback, TFeedback> WithFeedback<TFeedback>(TFeedback Feedback)
  {
    return new(FeedbackSource.FromInstance(Feedback));
  }

  public static ThoughtResult<TProduct, TFeedback> FromOutput<TProduct, TFeedback>(TProduct Product, TFeedback Feedback)
  {
    return new(Product, Feedback, null);
  }

  public static ThoughtResult<TProduct, TFeedback> FromException<TProduct, TFeedback>(
    TFeedback Feedback,
    Exception Exception)
  {
    return new(default, Feedback, ExceptionDispatchInfo.Capture(Exception));
  }

  public static WithFeedbackFactory<TConfigurator, TFeedback> WithFeedbackSource<TConfigurator, TFeedback>(
    FeedbackSource<TConfigurator, TFeedback> Source)
    => new(Source);

  public readonly struct WithFeedbackFactory<TConfigurator, TFeedback>(FeedbackSource<TConfigurator, TFeedback> Source)
  {
    public ThoughtResult<TProduct, TFeedback> FromLogic<TProduct>(
      Func<TConfigurator, TProduct> MakeProduct)
    {
      try
      {
        var Product = MakeProduct(Source.Configurator);
        var TFeedback = Source.CreateFeedback();
        return FromOutput(Product, TFeedback);
      }
      catch (Exception Ex)
      {
        return FromException<TProduct, TFeedback>(Source.CreateFeedback(), Ex);
      }
    }

    public ThoughtResult<TProduct, TFeedback> FromLogic<TProduct>(Func<TProduct> MakeProduct)
    {
      return FromLogic(_ => MakeProduct());
    }

    public async Task<ThoughtResult<TProduct, TFeedback>> FromLogicAsync<TProduct>(
      Func<TConfigurator, Task<TProduct>> MakeProduct)
    {
      try
      {
        var Product = await MakeProduct(Source.Configurator);
        return FromOutput(Product, Source.CreateFeedback());
      }
      catch (Exception Ex)
      {
        return FromException<TProduct, TFeedback>(Source.CreateFeedback(), Ex);
      }
    }

    public Task<ThoughtResult<TProduct, TFeedback>> FromLogicAsync<TProduct>(Func<Task<TProduct>> MakeProduct)
    {
      return FromLogicAsync(_ => MakeProduct());
    }
  }
}