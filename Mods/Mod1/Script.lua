buttonText = UE.GameObject.Find("Canvas/Buttons/ClickMe/Text"):GetComponent(typeof(UE.UI.Text))
eventSystem = UE.EventSystems.EventSystem.current
results = System.Collections.Generic.List(UE.EventSystems.RaycastResult)()
previousPos = UE.Input.mousePosition

function update()
	local mousePos = UE.Input.mousePosition
	
	local pointerData = UE.EventSystems.PointerEventData(eventSystem)
	pointerData.position = UE.Vector2(mousePos.x, mousePos.y)

	eventSystem:RaycastAll(pointerData, results)

	if results.Count > 0 and results[0].gameObject.name == "ClickMe" then
		transform = results[0].gameObject.transform
		transform.position = transform.position + mousePos - previousPos;
	end

	previousPos = mousePos;
end

function changeColor()
	buttonText.color = UE.Random.ColorHSV(0, 1, 0.75, 1, 0.75, 1);
end
invokeRepeating(changeColor, 0, 0.5)