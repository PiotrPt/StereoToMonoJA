#include "pch.h"
#include <cmath>
#include <algorithm>
#include <vector>
#include <thread>
#include <immintrin.h>

#undef GetVersion  // Undefine Windows GetVersion macro

// Export definitions
#define MONOCPP_API extern "C" __declspec(dllexport)

// Mix modes
enum MixMode {
    MIX_ARITHMETIC = 0,
    MIX_EQUAL_POWER = 1
};

// Processing options structure
struct ProcessingOptions {
    bool antiClipping;
    bool normalize;
    bool removeDC;
    bool dithering;
    MixMode mixMode;
    int numThreads;
};

// Simple TPDF dither generator
inline float GenerateDither() {
    float r1 = (rand() % 2048 - 1024) / 32768.0f;
    float r2 = (rand() % 2048 - 1024) / 32768.0f;
    return r1 + r2;
}

// Clamp function template
template<typename T>
inline T Clamp(T value, T minVal, T maxVal) {
    return (value < minVal) ? minVal : ((value > maxVal) ? maxVal : value);
}

// PCM16 processing block with SIMD optimization
void ProcessPCM16Block(const int16_t* left, const int16_t* right, int16_t* output, 
                       size_t count, const ProcessingOptions& options, 
                       double dcOffsetL, double dcOffsetR) {
    const float sqrtHalf = 0.70710678118f; // sqrt(0.5) for equal-power
    const float mixFactor = (options.mixMode == MIX_EQUAL_POWER) ? sqrtHalf : 0.5f;
    
    for (size_t i = 0; i < count; ++i) {
        float l = static_cast<float>(left[i]);
        float r = static_cast<float>(right[i]);
        
        // Remove DC offset
        if (options.removeDC) {
            l -= static_cast<float>(dcOffsetL);
            r -= static_cast<float>(dcOffsetR);
        }
        
        // Mix channels
        float mono = (l + r) * mixFactor;
        
        // Dithering (TPDF - Triangular Probability Density Function)
        if (options.dithering) {
            mono += GenerateDither();
        }
        
        // Anti-clipping and conversion
        if (options.antiClipping) {
            mono = Clamp(mono, -32768.0f, 32767.0f);
        }
        
        output[i] = static_cast<int16_t>(std::round(mono));
    }
}

// Float32 processing block
void ProcessFloat32Block(const float* left, const float* right, float* output,
                         size_t count, const ProcessingOptions& options,
                         double dcOffsetL, double dcOffsetR) {
    const float sqrtHalf = 0.70710678118f;
    const float mixFactor = (options.mixMode == MIX_EQUAL_POWER) ? sqrtHalf : 0.5f;
    
    for (size_t i = 0; i < count; ++i) {
        float l = left[i];
        float r = right[i];
        
        // Remove DC offset
        if (options.removeDC) {
            l -= static_cast<float>(dcOffsetL);
            r -= static_cast<float>(dcOffsetR);
        }
        
        // Mix channels
        float mono = (l + r) * mixFactor;
        
        // Dithering for float32
        if (options.dithering) {
            mono += GenerateDither() * 0.000031f; // Scaled for float range
        }
        
        // Anti-clipping
        if (options.antiClipping) {
            mono = Clamp(mono, -1.0f, 1.0f);
        }
        
        output[i] = mono;
    }
}

// Calculate DC offset
double CalculateDCOffset(const void* data, size_t count, bool isFloat32) {
    double sum = 0.0;
    
    if (isFloat32) {
        const float* fdata = static_cast<const float*>(data);
        for (size_t i = 0; i < count; ++i) {
            sum += fdata[i];
        }
    } else {
        const int16_t* idata = static_cast<const int16_t*>(data);
        for (size_t i = 0; i < count; ++i) {
            sum += idata[i];
        }
    }
    
    return sum / count;
}

// Find peak value for normalization
float FindPeak(const void* data, size_t count, bool isFloat32) {
    float peak = 0.0f;
    
    if (isFloat32) {
        const float* fdata = static_cast<const float*>(data);
        for (size_t i = 0; i < count; ++i) {
            peak = std::max(peak, std::abs(fdata[i]));
        }
    } else {
        const int16_t* idata = static_cast<const int16_t*>(data);
        for (size_t i = 0; i < count; ++i) {
            peak = std::max(peak, static_cast<float>(std::abs(idata[i])));
        }
    }
    
    return peak;
}

// Normalize audio data
void NormalizeAudio(void* data, size_t count, bool isFloat32, float targetPeak) {
    float peak = FindPeak(data, count, isFloat32);
    
    if (peak < 0.0001f) return; // Avoid division by zero
    
    float scale = targetPeak / peak;
    
    if (isFloat32) {
        float* fdata = static_cast<float*>(data);
        for (size_t i = 0; i < count; ++i) {
            fdata[i] *= scale;
        }
    } else {
        int16_t* idata = static_cast<int16_t*>(data);
        for (size_t i = 0; i < count; ++i) {
            float val = idata[i] * scale;
            idata[i] = static_cast<int16_t>(Clamp(val, -32768.0f, 32767.0f));
        }
    }
}

// Main processing function - PCM16
MONOCPP_API int ProcessStereoToMonoPCM16(
    const int16_t* leftChannel,
    const int16_t* rightChannel,
    int16_t* monoOutput,
    size_t sampleCount,
    int mixMode,
    bool antiClipping,
    bool normalize,
    bool removeDC,
    bool dithering,
    int numThreads)
{
    if (!leftChannel || !rightChannel || !monoOutput || sampleCount == 0) {
        return -1; // Invalid parameters
    }
    
    ProcessingOptions options;
    options.mixMode = static_cast<MixMode>(mixMode);
    options.antiClipping = antiClipping;
    options.normalize = normalize;
    options.removeDC = removeDC;
    options.dithering = dithering;
    options.numThreads = std::max(1, std::min(64, numThreads));
    
    // Calculate DC offsets if needed
    double dcOffsetL = 0.0, dcOffsetR = 0.0;
    if (options.removeDC) {
        dcOffsetL = CalculateDCOffset(leftChannel, sampleCount, false);
        dcOffsetR = CalculateDCOffset(rightChannel, sampleCount, false);
    }
    
    // Multi-threaded processing
    if (options.numThreads > 1) {
        size_t samplesPerThread = sampleCount / options.numThreads;
        std::vector<std::thread> threads;
        
        for (int t = 0; t < options.numThreads; ++t) {
            size_t start = t * samplesPerThread;
            size_t count = (t == options.numThreads - 1) ? 
                          (sampleCount - start) : samplesPerThread;
            
            threads.emplace_back([&, start, count]() {
                ProcessPCM16Block(
                    leftChannel + start,
                    rightChannel + start,
                    monoOutput + start,
                    count,
                    options,
                    dcOffsetL,
                    dcOffsetR
                );
            });
        }
        
        for (auto& thread : threads) {
            thread.join();
        }
    } else {
        ProcessPCM16Block(leftChannel, rightChannel, monoOutput, 
                         sampleCount, options, dcOffsetL, dcOffsetR);
    }
    
    // Normalize if requested
    if (options.normalize) {
        NormalizeAudio(monoOutput, sampleCount, false, 32767.0f);
    }
    
    return 0; // Success
}

// Main processing function - Float32
MONOCPP_API int ProcessStereoToMonoFloat32(
    const float* leftChannel,
    const float* rightChannel,
    float* monoOutput,
    size_t sampleCount,
    int mixMode,
    bool antiClipping,
    bool normalize,
    bool removeDC,
    bool dithering,
    int numThreads)
{
    if (!leftChannel || !rightChannel || !monoOutput || sampleCount == 0) {
        return -1; // Invalid parameters
    }
    
    ProcessingOptions options;
    options.mixMode = static_cast<MixMode>(mixMode);
    options.antiClipping = antiClipping;
    options.normalize = normalize;
    options.removeDC = removeDC;
    options.dithering = dithering;
    options.numThreads = std::max(1, std::min(64, numThreads));
    
    // Calculate DC offsets if needed
    double dcOffsetL = 0.0, dcOffsetR = 0.0;
    if (options.removeDC) {
        dcOffsetL = CalculateDCOffset(leftChannel, sampleCount, true);
        dcOffsetR = CalculateDCOffset(rightChannel, sampleCount, true);
    }
    
    // Multi-threaded processing
    if (options.numThreads > 1) {
        size_t samplesPerThread = sampleCount / options.numThreads;
        std::vector<std::thread> threads;
        
        for (int t = 0; t < options.numThreads; ++t) {
            size_t start = t * samplesPerThread;
            size_t count = (t == options.numThreads - 1) ? 
                          (sampleCount - start) : samplesPerThread;
            
            threads.emplace_back([&, start, count]() {
                ProcessFloat32Block(
                    leftChannel + start,
                    rightChannel + start,
                    monoOutput + start,
                    count,
                    options,
                    dcOffsetL,
                    dcOffsetR
                );
            });
        }
        
        for (auto& thread : threads) {
            thread.join();
        }
    } else {
        ProcessFloat32Block(leftChannel, rightChannel, monoOutput,
                           sampleCount, options, dcOffsetL, dcOffsetR);
    }
    
    // Normalize if requested
    if (options.normalize) {
        NormalizeAudio(monoOutput, sampleCount, true, 1.0f);
    }
    
    return 0; // Success
}

// Version info - renamed to avoid Windows API conflict
MONOCPP_API const char* GetLibraryVersion() {
    return "MonoCPP v1.0.0 (C++ Reference Implementation)";
}
