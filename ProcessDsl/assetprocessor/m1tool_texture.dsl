input("*.tga","*.png","*.jpg")
{
	string("filter", "");
	int("maxSize",1);
	feature("source", "project");
	feature("menu", "0.Asset Processors/M1Tool Texture Setting");
	feature("description", "just so so");
}
filter
{
	var(0) = loadasset(assetpath);
	var(1) = var(0).width;
	var(2) = var(0).height;
	//unloadasset(var(0));
	if((var(1) > maxSize || var(2) > maxSize) && assetpath.Contains(filter) && !assetpath.Contains("Materials")){
		info = "size:" + var(1) + "," + var(2);
		1;
	} else {
		0;
	};
}
assetprocessor
{
	CorrectNoneAlphaTexture;
	SetAndroidASTC;
	SetIPhoneASTC;
	SetCompressed;
	SetReadableFalse;
	SetMipMapTrue;
	SetFilterBilinear;
	SetStreamingMipMapsTrue;
	SetAnisoLevelN1;
	SetDirty;
	SaveAndReimport;
};