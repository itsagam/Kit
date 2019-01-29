local ToLuaInjectionTestInjector = {}

--ToLuaInjectionTestInjector[".ctor"] = function()
	--请注意ToLuaInjection.cs中的injectIgnoring字段已经默认过滤掉了Constructor，如果要测试构造函数的注入，请去掉InjectFilter.IgnoreConstructor这个过滤项
	-- Only After Does Matter
	--return function(self)
	--	print("Lua Inject Constructor")
	--end, LuaInterface.InjectType.After
	-------------------------------------------------------
	--return function(self)
	--	print("Lua Inject Constructor")
	--end, LuaInterface.InjectType.Before
	-------------------------------------------------------
--end

--ToLuaInjectionTestInjector[".ctor_bool"] = function()
	--请注意ToLuaInjection.cs中的injectIgnoring字段已经默认过滤掉了Constructor，如果要测试构造函数的注入，请去掉InjectFilter.IgnoreConstructor这个过滤项
	-- Only After Does Matter
	--return function(self, state)
	--	print("Lua Inject Constructor_bool " .. tostring(state))
	--end, LuaInterface.InjectType.After
--end

ToLuaInjectionTestInjector.set_PropertyTest = function()
	return function (self, value)
		print("Lua Inject Property set :" .. value)
	end, LuaInterface.InjectType.After
	-------------------------------------------------------
	--return function (self, value)
	--	print("Lua Inject Property set :")
	--	return {3}
	--end, LuaInterface.InjectType.Replace
	-------------------------------------------------------
end

ToLuaInjectionTestInjector.get_PropertyTest = function()
	return function (self)
		print("Lua Inject Property get :")
	end, LuaInterface.InjectType.After
	-------------------------------------------------------
	--return function (self)
	--	print("Lua Inject Property get :")
	--end, LuaInterface.InjectType.Before
	-------------------------------------------------------
	--return function (self)
	--	print("Lua Inject Property get :")
	--	return 2
	--end, LuaInterface.InjectType.Replace
	-------------------------------------------------------
end

ToLuaInjectionTestInjector.TestRef = function()
	--return function (self, count)
	--	print("Lua Inject TestRef ")
	--	count = 10
	--	return { count , 3}
	--end, LuaInterface.InjectType.After
	-------------------------------------------------------
	--return function (self, count)
	--	print("Lua Inject TestRef ")
	--	count = 10
	--	return { count , 3}
	--end, LuaInterface.InjectType.ReplaceWithPreInvokeBase
	-------------------------------------------------------
	return function (self, count)
		print("Lua Inject TestRef ")
		count = 10
		return { count , 3}
	end, LuaInterface.InjectType.ReplaceWithPostInvokeBase
	-------------------------------------------------------
	--return function (self, count)
	--	print("Lua Inject TestRef ")
	--	count = 10
	--	return { count , 3}
	--end, LuaInterface.InjectType.Replace
	-------------------------------------------------------
	--return function (self, count)
	--	print("Lua Inject TestRef aaaaaaaaaaaaaaaaaaaaaaaa ")
	--	count = 10
	--	return { count , 3}
	--end, LuaInterface.InjectType.Before
	-------------------------------------------------------
end

ToLuaInjectionTestInjector["TestOverload-int-bool"] = function()
	return function (self, count, state)
		print("Agam Saranbbbbbbbbbbbbbbbbbbbbbbbbbbbbb" .. tostring(state))
	end, LuaInterface.InjectType.After
end

ToLuaInjectionTestInjector["TestOverload-int-bool&"] = function()
	--return function (self, param1, param2)
	--	print("Lua Inject TestOverload-int-bool& ")
	--	return {false}
	--end, LuaInterface.InjectType.After
	-------------------------------------------------------
	--return function (self, param1, param2)
	--	print("Lua Inject TestOverload-int-bool& ")
	--	return {false}
	--end, LuaInterface.InjectType.Before
	-------------------------------------------------------
	return function (self, param1, param2)
		print("Lua Inject TestOverload-int-bool& ")
		return {false}
	end, LuaInterface.InjectType.Replace
	-------------------------------------------------------
end

ToLuaInjectionTestInjector["TestOverload-bool-int"] = function()
	return function (self, param1, param2)
		print("Lua Inject TestOverload-bool-int " .. param2)
	end, LuaInterface.InjectType.After
end

ToLuaInjectionTestInjector.TestCoroutine = function()
	return function (self, delay, coroutineState)
		print("Lua Inject TestCoroutine " .. coroutineState)
	end, LuaInterface.InjectType.After
	-------------------------------------------------------
	--return function (self, delay)
	--	return WrapLuaCoroutine(function()
	--		print("Lua Inject TestCoroutine Pulse" .. delay)
	--		return false
	--	end)
	--end, LuaInterface.InjectType.Replace
	-------------------------------------------------------
	--return function (self, delay)
	--	local state = true
	--	local cor
	--	local function StartLuaCoroutine()
	--		if cor == nil then
	--			cor = coroutine.start(function()
	--				print("Lua Coroutine Before")
	--				coroutine.wait(delay)
	--				state = false
	--				print("Lua Coroutine After")
	--			end)
	--		end
	--	end
	--
	--	return WrapLuaCoroutine(function()
	--		StartLuaCoroutine()
	--		return state
	--	end)
	--end, LuaInterface.InjectType.Replace
	-------------------------------------------------------
end

--InjectByName("ToLuaInjectionTest", ToLuaInjectionTestInjector)
InjectByModule(ToLuaInjectionTest, ToLuaInjectionTestInjector)