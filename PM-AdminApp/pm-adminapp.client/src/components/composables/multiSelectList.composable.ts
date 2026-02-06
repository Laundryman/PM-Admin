////////////////////////////////////////////////////
// Generic Multi-Select Handler
///////////////////////////////////////////////////
export function useMultiSelectLists() {
  function manageSelectedValues(
    selectedValues: number[],
    availableValues: any[],
    targetArray: any[],
  ) {
    if (selectedValues.length > 0) {
      for (const valueId of selectedValues) {
        let foundValue = targetArray.find((st) => st.id === valueId)
        if (!foundValue) {
          //handling unpublished items in the target array
          let item = availableValues?.find((st) => st.id === valueId)
          if (item) targetArray?.push(item)
        }
      }
    }
    let selectedValuesToRemove = new Array<number>()
    for (const item of targetArray ?? []) {
      let index = selectedValues.indexOf(item.id) //if it's been removed
      if (index == -1) {
        // let foundItem = availableValues.find((st) => st.id === item.id)
        // if (!foundItem)
        var published = item.published ?? true
        if (published === true) {
          selectedValuesToRemove.push(item.id)
        }
      }
    }
    if (selectedValuesToRemove.length > 0) {
      for (const stId of selectedValuesToRemove) {
        let removeIndex = targetArray.findIndex((st) => st.id === stId)
        if (removeIndex !== -1) {
          targetArray.splice(removeIndex, 1)
        }
      }
    }
  }
  return { manageSelectedValues }
}
