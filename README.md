<p align="center" width="100%">
    <img width="25%" src="Assets/Amuse-Logo-512.png">
</p> 

## Required Dependencies
1. `Microsoft.ML.OnnxRuntime.Managed` v1.23.0-dev-20250603-0558-cd5133371
2. `Microsoft.ML.OnnxRuntime.MIGraphX.Windows` v1.23.0-dev-20250603-0558-cd5133371

## Required External Plugins
1. `ContentFilter` add `ContentFilter.onnx` & `ContentFilter.bin` to `Plugins\ContentFilter`
2. `CLIPTokenizer` add tokenizer files to `Plugins\CLIPTokenizer`
3. `RyzenAI` add `RyzenAI v1.5` xclbin files to `Plugins\RyzenAI` (extract these from the RyzenAI python package)
4. `SuperResolution` not sure where to get these files publically so grab from Amuse latest installer

Note: Easy way is to just install the latest Amuse version and copy the files from the `X:\Program Files\Amuse\Plugins` directory


## Required External Licences

`ImageSharp v3` Licence Required https://sixlabors.com/pricing/
1. Add licence file to root project directory

`FontAwesome Pro v6` Licence Required https://fontawesome.com/v6/download
1. Download the `6.7.2 for the desktop` package
2. Add the font files to the `Fonts` directory
3. Set build action to `Resource` for all font files