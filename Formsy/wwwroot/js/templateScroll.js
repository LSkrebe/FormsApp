document.addEventListener("DOMContentLoaded", function () {
    let page = 1;
    const container = document.getElementById("templateContainer");
    const loading = document.getElementById("loading");
    let isLoading = false;
    let hasMoreTemplates = true;

    async function loadTemplates() {
        if (isLoading || !hasMoreTemplates) return;

        isLoading = true;
        loading.style.display = "block";

        try {
            const response = await fetch(`/Template/LoadTemplates?page=${page}`);
            const templates = await response.json();

            if (templates.length === 0) {
                hasMoreTemplates = false;
            } else {
                templates.forEach(template => {
                    const card = document.createElement("div");
                    card.classList.add("col-md-4", "mb-4");
                    card.innerHTML = `
                        <a href="/Form/Fill/@template.Id" class="card-link">
                            <div class="form-card">
                                <h3>@Truncate(template.Title, 50)</h3>
                                <p>@Truncate(template.Description, 100)</p>
                                <p><strong>@template.Questions.Count Questions</strong></p>
                                <p><strong>@template.Forms.Count Replies</strong></p>
                            </div>
                        </a>
                    `;
                    container.appendChild(card);
                });
                page++;
            }
        } catch (error) {
            console.error("Error loading templates:", error);
        } finally {
            isLoading = false;

            setTimeout(() => {
                loading.style.display = "none";
            }, 300);
        }
    }

    function truncate(text, length) {
        if (text.length <= length) return text;
        return text.substring(0, length) + "...";
    }

    window.addEventListener("scroll", function () {
        if (window.innerHeight + window.scrollY >= document.body.offsetHeight - 100) {
            loadTemplates();
        }
    });

    loadTemplates();
});
