.CODE

; Constants alignment
ALIGN 16

; ProcessStereoToMonoPCM16_AVX2
; Windows x64 calling convention:
; RCX = leftChannel (const int16_t*)
; RDX = rightChannel (const int16_t*)
; R8 = monoOutput (int16_t*)
; R9 = sampleCount (size_t)
; Stack: [RSP+28h] = mixMode, [RSP+30h] = antiClipping, etc.

PUBLIC ProcessStereoToMonoPCM16_AVX2
ProcessStereoToMonoPCM16_AVX2 PROC
    ; Save non-volatile registers
    push rbx
    push rsi
    push rdi
    push r12
    push r13
    push r14
    push r15
    sub rsp, 40h
    
    ; Save parameters
    mov rsi, rcx                ; leftChannel
    mov rdi, rdx                ; rightChannel
    mov rbx, r8                 ; monoOutput
    mov r12, r9                 ; sampleCount
    
    ; Load mixing constants
    mov eax, DWORD PTR [rsp+98h]  ; mixMode from stack
    test eax, eax
    jz UseArithmeticMix
    
    ; Equal-power mixing: sqrt(0.5) = 0.707...
    vmovss xmm6, DWORD PTR [sqrtHalf]
    jmp MixConstantLoaded
    
UseArithmeticMix:
    ; Arithmetic mixing: 0.5
    vmovss xmm6, DWORD PTR [half]
    
MixConstantLoaded:
    vshufps xmm6, xmm6, xmm6, 0  ; Broadcast to all 4 elements
    vinsertf128 ymm6, ymm6, xmm6, 1  ; Broadcast to all 8 elements
    
    ; Process 16 samples at a time with AVX2
    mov r13, r12
    shr r13, 4                  ; Divide by 16
    test r13, r13
    jz ProcessRemaining
    
ProcessLoop16:
    ; Load 16 int16 samples from left channel (32 bytes)
    vmovdqu ymm0, YMMWORD PTR [rsi]
    
    ; Load 16 int16 samples from right channel
    vmovdqu ymm1, YMMWORD PTR [rdi]
    
    ; Convert lower 8 int16 to int32
    vpmovsxwd ymm2, xmm0
    vpmovsxwd ymm3, xmm1
    
    ; Convert int32 to float
    vcvtdq2ps ymm2, ymm2
    vcvtdq2ps ymm3, ymm3
    
    ; Add left and right
    vaddps ymm4, ymm2, ymm3
    
    ; Multiply by mix factor
    vmulps ymm4, ymm4, ymm6
    
    ; Convert back to int32
    vcvtps2dq ymm4, ymm4
    
    ; Process upper 8 samples
    vextracti128 xmm0, ymm0, 1
    vextracti128 xmm1, ymm1, 1
    
    vpmovsxwd ymm2, xmm0
    vpmovsxwd ymm3, xmm1
    
    vcvtdq2ps ymm2, ymm2
    vcvtdq2ps ymm3, ymm3
    
    vaddps ymm5, ymm2, ymm3
    vmulps ymm5, ymm5, ymm6
    vcvtps2dq ymm5, ymm5
    
    ; Pack both results: ymm4 (lower 8) and ymm5 (upper 8) into int16
    vpackssdw ymm4, ymm4, ymm5
    vpermq ymm4, ymm4, 0D8h      ; Reorder to correct positions
    
    ; Store 16 int16 results
    vmovdqu YMMWORD PTR [rbx], ymm4
    
    ; Advance pointers
    add rsi, 32                  ; 16 samples * 2 bytes
    add rdi, 32
    add rbx, 32
    
    dec r13
    jnz ProcessLoop16
    
ProcessRemaining:
    ; Process remaining samples (scalar)
    mov r14, r12
    and r14, 0Fh                 ; Remaining samples (modulo 16)
    test r14, r14
    jz Done
    
RemainLoop:
    movsx eax, WORD PTR [rsi]    ; Load left sample (sign-extend)
    movsx edx, WORD PTR [rdi]    ; Load right sample
    
    cvtsi2ss xmm0, eax           ; Convert to float
    cvtsi2ss xmm1, edx
    
    addss xmm0, xmm1             ; Add
    mulss xmm0, xmm6             ; Multiply by mix factor
    
    cvtss2si eax, xmm0           ; Convert back to int
    
    ; Clamp to int16 range [-32768, 32767]
    cmp eax, 32767
    jle NotTooHigh
    mov eax, 32767
NotTooHigh:
    cmp eax, -32768
    jge NotTooLow
    mov eax, -32768
NotTooLow:
    
    mov WORD PTR [rbx], ax       ; Store result
    
    add rsi, 2
    add rdi, 2
    add rbx, 2
    
    dec r14
    jnz RemainLoop
    
Done:
    vzeroupper
    
    ; Restore registers and return
    add rsp, 40h
    pop r15
    pop r14
    pop r13
    pop r12
    pop rdi
    pop rsi
    pop rbx
    
    xor rax, rax                 ; Return 0 (success)
    ret
ProcessStereoToMonoPCM16_AVX2 ENDP

; ProcessStereoToMonoFloat32_AVX2
PUBLIC ProcessStereoToMonoFloat32_AVX2
ProcessStereoToMonoFloat32_AVX2 PROC
    push rbx
    push rsi
    push rdi
    push r12
    push r13
    sub rsp, 40h
    
    mov rsi, rcx                 ; leftChannel
    mov rdi, rdx                 ; rightChannel
    mov rbx, r8                  ; monoOutput
    mov r12, r9                  ; sampleCount
    
    ; Load mixing constant
    mov eax, DWORD PTR [rsp+90h] ; mixMode
    test eax, eax
    jz UseArithmeticMixFloat
    
    vmovss xmm6, DWORD PTR [sqrtHalf]
    jmp MixConstantLoadedFloat
    
UseArithmeticMixFloat:
    vmovss xmm6, DWORD PTR [half]
    
MixConstantLoadedFloat:
    vshufps xmm6, xmm6, xmm6, 0
    vinsertf128 ymm6, ymm6, xmm6, 1
    
    ; Process 8 samples at a time
    mov r13, r12
    shr r13, 3                   ; Divide by 8
    test r13, r13
    jz ProcessRemainingFloat
    
ProcessLoopFloat8:
    vmovups ymm0, YMMWORD PTR [rsi]     ; Load 8 floats from left
    vmovups ymm1, YMMWORD PTR [rdi]     ; Load 8 floats from right
    
    vaddps ymm2, ymm0, ymm1              ; Add
    vmulps ymm2, ymm2, ymm6              ; Multiply by mix factor
    
    vmovups YMMWORD PTR [rbx], ymm2      ; Store result
    
    add rsi, 32                           ; 8 samples * 4 bytes
    add rdi, 32
    add rbx, 32
    
    dec r13
    jnz ProcessLoopFloat8
    
ProcessRemainingFloat:
    mov r13, r12
    and r13, 7                    ; Remaining samples
    test r13, r13
    jz DoneFloat
    
RemainLoopFloat:
    movss xmm0, DWORD PTR [rsi]
    movss xmm1, DWORD PTR [rdi]
    
    addss xmm0, xmm1
    mulss xmm0, xmm6
    
    movss DWORD PTR [rbx], xmm0
    
    add rsi, 4
    add rdi, 4
    add rbx, 4
    
    dec r13
    jnz RemainLoopFloat
    
DoneFloat:
    vzeroupper
    
    add rsp, 40h
    pop r13
    pop r12
    pop rdi
    pop rsi
    pop rbx
    
    xor rax, rax
    ret
ProcessStereoToMonoFloat32_AVX2 ENDP

; GetVersion_ASM
PUBLIC GetVersion_ASM
GetVersion_ASM PROC
    lea rax, versionString
    ret
GetVersion_ASM ENDP

.DATA
ALIGN 16
sqrtHalf REAL4 0.70710678
half REAL4 0.5
versionString DB "MonoASM v1.0.0 (x64 AVX2 Optimized)", 0

END
