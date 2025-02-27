﻿#version 450 core
layout (location = 0) in vec2 position;
layout (location = 1) in vec2 TexCoord;
out vec2 TexCoord0;

layout (location = 20) uniform  mat4 projection;
layout (location = 21) uniform  mat4 charOffset;
void main(void)
{
    gl_Position = projection*charOffset*vec4(position,-1f,1.0f);
    TexCoord0 = vec2(TexCoord);
}