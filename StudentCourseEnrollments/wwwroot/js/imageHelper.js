window.createDynamicImage = (imagePath, attributesJson = null) => {
    const container = document.getElementById('img');
    if (!container) {
        console.warn('Dynamic image container (#img) not found in DOM');
        return;
    }

    container.innerHTML = '';

    if (!imagePath) return;

    const img = document.createElement('img');
    img.src = imagePath;
    img.alt = 'Login feedback';
    img.className = 'img-fluid rounded-lg';
    img.style.maxWidth = '300px';
    img.style.marginTop = '20px';
    img.style.display = 'block';
    img.style.marginLeft = 'auto';
    img.style.marginRight = 'auto';

    if (attributesJson) {
        try {
            const attrs = JSON.parse(attributesJson);
            Object.entries(attrs).forEach(([key, value]) => {
                img.setAttribute(key, value);
            });
        } catch (e) {
            console.error('Failed to parse attributes JSON', e);
        }
    }

    container.appendChild(img);
};