-- There are a lot of ways of scheduling a function:
-- * You can implement awake/start/update/... methods and use Full persistence
-- * You can use schedule(type)
-- * You can use startCoroutine
-- * You can call invoke and invokeRepeating
-- * Or you can handle it manually

function update()
	mousePos = UE.Input.mousePosition

end

inject(typeof(CS.Demos.Modding.Demo), "InjectedReplace",
function ()
	Kit.SceneDirector.LoadScene("Menu")
end)

inject(typeof(CS.Demos.Modding.Demo), "InjectedExtend",
function (demo)
	demo:InjectedExtend()
	print(self.Name .. " Code")
	demo:InjectedExtend()
end)