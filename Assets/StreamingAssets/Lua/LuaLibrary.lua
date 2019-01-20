UE = CS.UnityEngine

function GO(path, comp)
	if type(path) == "string" then
		if comp == nil then
			return UE.GameObject.Find(path)
		else
			return UE.GameObject.Find(path):GetComponent(typeof(comp))
		end
	else
		return UE.GameObject.FindObjectOfType(typeof(path))
	end
end

function GOs(tag)
	if type(tag) == "string" then
		return UE.GameObject.FindGameObjectsWithTag(tag)
	else
		return UE.GameObject.FindObjectsOfType(typeof(tag))
	end
end

function Create(name, ...)
	go = UE.GameObject(name)
	for k, v in pairs({...}) do
		go:AddComponent(typeof(v))
	end
	return go
end

function Instantiate(path)
	prefab = UE.Resources.Load(path)
	if prefab ~= nil then
		return UE.GameObject.Instantiate(prefab);
	else
		return nil
	end
end

function Destroy(path, comp)
	UE.GameObject.Destroy(GO(path, comp))
end