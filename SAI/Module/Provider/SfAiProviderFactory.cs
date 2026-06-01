using System;
using SFramework.SAI.Module.Data;

namespace SFramework.SAI.Module.Provider
{
  public static class SfAiProviderFactory
  {
    public static ISfAiProvider Create(SfAiProviderProfile profile)
    {
      if (profile == null) throw new ArgumentNullException(nameof(profile));

      return profile.GetResolvedType() switch
      {
        SfAiProviderType.Anthropic => new SfAiAnthropicProvider(),
        SfAiProviderType.Ollama => new SfAiOllamaProvider(),
        SfAiProviderType.OpenAICompatible => new SfAiOpenAiCompatibleProvider(),
        SfAiProviderType.Custom => new SfAiOpenAiCompatibleProvider(),
        _ => new SfAiOpenAiCompatibleProvider()
      };
    }
  }
}
