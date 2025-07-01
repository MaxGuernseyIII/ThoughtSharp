namespace ThoughtSharp.Runtime;

/// <summary>
/// The primary interface into an adapter library, such as the library currently offered for TorchSharp.
/// </summary>
/// <typeparam name="TBrain">The type of brain to produce.</typeparam>
/// <typeparam name="TModel">The type of model to use in the brain.</typeparam>
/// <typeparam name="TDevice">The type of device to use in the brain.</typeparam>
// TODO: Think about getting rid of TBrain
public interface BrainFactory<
  out TBrain, 
  TModel,
  TDevice>
{
  /// <summary>
  /// Build the brain object out of <see cref="Model"/> amd <see cref="Device"/>.
  /// </summary>
  /// <param name="Model">The model that serves as the core of the brain</param>
  /// <param name="Device">The device upon which the brain executes</param>
  /// <returns>The requested brain</returns>
  TBrain CreateBrain(TModel Model, TDevice Device);

  /// <summary>
  /// Creates a sequential model.
  /// </summary>
  /// <param name="Children">The list of children to execute in sequence</param>
  /// <returns>The requested model</returns>
  TModel CreateSequence(params IEnumerable<TModel> Children);

  /// <summary>
  /// Creates a parallel model.
  /// </summary>
  /// <param name="Children">The list of children to execute in parallel</param>
  /// <returns>The requested model</returns>
  TModel CreateParallel(params IEnumerable<TModel> Children);

  /// <summary>
  /// Creates a time-aware model, which processes time-sequences of steps and then reduces them to a single timestep for subsequent processing.
  /// </summary>
  /// <param name="Children">The list of children to execute on the time steps</param>
  /// <param name="Pooling">The final operation to reduce the time steps to a single timestep output</param>
  /// <returns>The requested model</returns>
  TModel CreateTimeAware(IEnumerable<TModel> Children, TModel Pooling);

  /// <summary>
  /// Creates a linear model.
  /// </summary>
  /// <param name="InputFeatures">The number of features the linear model should accept as input</param>
  /// <param name="OutputFeatures">The number of features the linear model should produce as output</param>
  /// <param name="WithBias">Whether there is bias</param>
  /// <returns>The requested model</returns>
  TModel CreateLinear(int InputFeatures, int OutputFeatures, bool WithBias);

  /// <summary>
  /// Creates a GRU model with an adapter from a number of input features.
  /// </summary>
  /// <param name="InputFeatures">The number of input features.</param>
  /// <param name="OutputFeatures">The number of output features.</param>
  /// <param name="GRULayers">The number of layers in the GRU.</param>
  /// <param name="Bidirectional">Whether the GRU is forward-only or bidirectional</param>
  /// <param name="Device">The device on which the GRU should execute.</param>
  /// <returns>The requested model</returns>
  TModel CreateGRU(int InputFeatures, int OutputFeatures, int GRULayers, bool Bidirectional, TDevice Device);

  /// <summary>
  /// Creates a multi-headed attention layer with any necessary adaptive linear layers.
  /// </summary>
  /// <param name="InputFeatures">The number of features to accept as input</param>
  /// <param name="Heads">The number of heads to produce as output</param>
  /// <param name="FeaturesPerHead">The number of features per output head</param>
  /// <returns>The requested model</returns>
  TModel CreateMultiHeadedAttention(int InputFeatures, int Heads, int FeaturesPerHead);

  /// <summary>
  /// Creates a dropout layer that will be active during training, but not during inference.
  /// </summary>
  /// <param name="Rate">The rate of dropout.</param>
  /// <returns>The requested model</returns>
  TModel CreateDropout(float Rate);

  /// <summary>
  /// Creates a layer norm layer.
  /// </summary>
  /// <param name="InputFeatures">The number of features to accept as input (and produce as output)</param>
  /// <returns>The requested model</returns>
  TModel CreateLayerNorm(int InputFeatures);

  /// <summary>
  /// Creates a pooling model that selects the last time step out of each sequence.
  /// </summary>
  /// <returns>The requested model</returns>
  TModel CreateLastTimeStep();

  /// <summary>
  /// Creates a pooling model that creates a masked mean over each relevant timestep in each sequence.
  /// </summary>
  /// <returns>The requested model</returns>
  TModel CreateMeanOverTimeStepsPooling();

  /// <summary>
  /// Creates an attention pooling model that uses learned weights to average over relevant time sequences.
  /// </summary>
  /// <param name="InputFeatures">The number of features to accept as input</param>
  /// <returns>The requested model</returns>
  TModel CreateAttentionPooling(int InputFeatures);

  /// <summary>
  /// Creates a tanh activation layer.
  /// </summary>
  /// <returns>The requested model</returns>
  TModel CreateTanh();

  /// <summary>
  /// Creates a ReLU activation layer.
  /// </summary>
  /// <returns>The requested model</returns>
  TModel CreateReLU();


  /// <summary>
  /// Creates a SiLU activation layer.
  /// </summary>
  /// <returns>The requested model</returns>
  TModel CreateSiLU();

  /// <summary>
  /// Creates a virtual device that selects the "best option" available, preferring CUDA to CPU.
  /// </summary>
  /// <returns>The requested device</returns>
  TDevice GetDefaultOptimumDevice();

  /// <summary>
  /// Creates a device that only allows execution on the CPU.
  /// </summary>
  /// <returns>The requested device</returns>
  TDevice GetCPUDevice();

  /// <summary>
  /// Creates a device that only allows execution on CUDA.
  /// </summary>
  /// <returns>The requested device</returns>
  TDevice GetCUDADevice();
}