i = 0

print("outer")

function update()
	print("update")
end

invokeRepeating(0.1, 0.1,
function()
	print("repeating")
end)