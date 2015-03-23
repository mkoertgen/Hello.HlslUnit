float4x4 ViewProj;
TextureCube CubeMap;

SamplerState CubeSampler
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexIn
{
	float3 PosL : SV_POSITION;
};

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float3 PosL : TEXCOORD0;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;
	vout.PosH = mul(float4(vin.PosL, 1.0f), ViewProj);
	vout.PosL = vin.PosL;
	return vout;
}

float4 PS(VertexOut pin) : SV_Target
{
	return CubeMap.Sample(CubeSampler, pin.PosL);
}

technique SkyBox
{
	pass SkyBox
	{
		Profile = 10.0;
		VertexShader = VS;
		PixelShader = PS;
	}
}
