document.addEventListener("DOMContentLoaded", function () {
    const container = document.getElementById("questionsContainer");
    const addButton = document.getElementById("addQuestion");

    // Limits per question type
    const typeLimits = {
        "Single Line": 4,
        "Multi Line": 4,
        "Number": 4,
        "Checkbox": 4
    };

    function countQuestionsByType() {
        const counts = { "Single Line": 0, "Multi Line": 0, "Number": 0, "Checkbox": 0 };
        document.querySelectorAll(".question-item select").forEach(select => {
            const type = select.value;
            if (counts[type] !== undefined) counts[type]++;
        });
        return counts;
    }

    function getFirstAvailableType() {
        const counts = countQuestionsByType();
        for (const type in typeLimits) {
            if (counts[type] < typeLimits[type]) {
                return type;
            }
        }
        return null;
    }

    function updateAddButtonVisibility() {
        const availableType = getFirstAvailableType();
        if (!availableType) {
            addButton.style.display = 'none';
        } else {
            addButton.style.display = 'inline-block';
        }
    }

    addButton.addEventListener("click", function () {
        const index = document.querySelectorAll(".question-item").length;
        const availableType = getFirstAvailableType();

        if (!availableType) {
            alert("You have reached the limit for all question types.");
            return;
        }

        const questionHtml = `
            <div class="question-item d-flex align-items-center mb-3">
                <input type="text" name="Questions[${index}].Text" class="form-control me-2" placeholder="Enter Question" required />

                <select name="Questions[${index}].Type" class="form-select me-2" onchange="handleTypeChange(this)">
                    ${generateOptions(availableType)}
                </select>

                <button type="button" class="btn btn-sm remove-question">Remove</button>
            </div>`;

        container.insertAdjacentHTML("beforeend", questionHtml);
        attachRemoveHandlers();
        updateAddButtonVisibility();
    });

    function generateOptions(selectedType) {
        return Object.keys(typeLimits)
            .map(type => `<option value="${type}" ${type === selectedType ? "selected" : ""}>${type}</option>`)
            .join("");
    }

    function attachRemoveHandlers() {
        document.querySelectorAll(".remove-question").forEach(button => {
            button.onclick = function () {
                this.parentElement.remove();
                updateAddButtonVisibility();
            };
        });
    }

    window.handleTypeChange = function (select) {
        const selectedType = select.value;
        const counts = countQuestionsByType();

        if (counts[selectedType] > typeLimits[selectedType]) {
            const newType = getFirstAvailableType();
            if (newType) {
                select.value = newType;
            } else {
                alert("No available question types left.");
                select.value = "Single Line";
            }
        }
    };

    attachRemoveHandlers();
    updateAddButtonVisibility();
});
