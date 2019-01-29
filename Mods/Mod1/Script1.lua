print("Hello from Mod1 script 1")

schedule("update", function ()
	print("hello")
end)

function update()
	print(self.Metadata.Name)
end

function destroy()
	--s1:Dispose()
	print("You unloaded me!")
end