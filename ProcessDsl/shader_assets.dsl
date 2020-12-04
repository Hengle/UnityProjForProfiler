input("t:shader")
{
	string("filter", "");
	feature("source", "allassets");
	feature("menu", "5.All Assets/Shaders");
	feature("description", "just so so");
}
filter
{
  if(assetpath.Contains(filter)){
  	var(0) = loadasset(assetpath);
  	order = getshaderpropertycount(var(0));
  	info = format("{0} property count:{1} texture count:{2}", getfilename(assetpath), order, getshaderpropertycount(var(0), 4));
    1;
  }else{
    0;
  };
}
process
{
  var(0) = loadasset(assetpath);
	addshadertocollection(var(0));
};