/**
 * Xtal Player - Main JavaScript Module
 * Modern audio player with Web Audio API
 */

class XtalPlayer {
    constructor() {
        this.audioContext = null;
        this.sourceNode = null;
        this.audioBuffer = null;
        this.isPlaying = false;
        this.startTime = 0;
        this.pauseOffset = 0;
        this.duration = 0;
        this.animationFrameId = null;
        
        this.elements = this.initializeElements();
        this.bindEvents();
        this.initializeUI();
    }

    /**
     * Initialize DOM elements
     */
    initializeElements() {
        return {
            audioFileInput: document.getElementById('audioFileInput'),
            playPauseButton: document.getElementById('playPause'),
            seekBar: document.getElementById('seekBar'),
            timeDisplay: document.getElementById('timeDisplay'),
            speedControl: document.getElementById('speedControl'),
            speedValue: document.getElementById('speedValue'),
            coverArt: document.getElementById('coverArt'),
            trackTitle: document.getElementById('trackTitle'),
            container: document.querySelector('.container')
        };
    }

    /**
     * Initialize UI state
     */
    initializeUI() {
        if (this.elements.speedValue) {
            this.elements.speedValue.textContent = '1.00×';
        }
        this.updateTimer(0);
    }

    /**
     * Bind event listeners
     */
    bindEvents() {
        // File input
        this.elements.audioFileInput?.addEventListener('change', (e) => this.handleFileInput(e));
        
        // Play/Pause button
        this.elements.playPauseButton?.addEventListener('click', () => this.togglePlayPause());
        
        // Seek bar
        this.elements.seekBar?.addEventListener('input', (e) => this.handleSeek(e));
        
        // Speed control
        this.elements.speedControl?.addEventListener('input', (e) => this.handleSpeedChange(e));
        
        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => this.handleKeyboard(e));
        
        // Page unload cleanup
        window.addEventListener('beforeunload', () => this.cleanup());
        
        // Audio context suspension handling
        document.addEventListener('click', () => this.resumeAudioContext(), { once: true });
        document.addEventListener('touchstart', () => this.resumeAudioContext(), { once: true });
    }

    /**
     * Initialize audio context on first user interaction
     */
    async initAudioContext() {
        if (!this.audioContext) {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)();
        }
        return this.audioContext;
    }

    /**
     * Resume audio context if suspended
     */
    async resumeAudioContext() {
        if (this.audioContext && this.audioContext.state === 'suspended') {
            await this.audioContext.resume();
        }
    }

    /**
     * Format time in MM:SS format
     */
    formatTime(seconds) {
        const minutes = Math.floor(seconds / 60);
        const secs = Math.floor(seconds % 60);
        return `${minutes}:${secs.toString().padStart(2, '0')}`;
    }

    /**
     * Update timer display
     */
    updateTimer(time) {
        const currentTime = Math.min(time, this.duration);
        if (this.elements.timeDisplay) {
            this.elements.timeDisplay.textContent = `${this.formatTime(currentTime)} / ${this.formatTime(this.duration)}`;
        }
    }

    /**
     * Update seek bar position
     */
    updateSeekBar() {
        if (this.isPlaying && this.audioContext) {
            const currentTime = this.audioContext.currentTime - this.startTime;
            if (this.elements.seekBar) {
                this.elements.seekBar.value = currentTime;
            }
            this.updateTimer(currentTime);
            this.animationFrameId = requestAnimationFrame(() => this.updateSeekBar());
        }
    }

    /**
     * Reset UI to initial state
     */
    resetUI() {
        this.isPlaying = false;
        if (this.elements.playPauseButton) {
            this.elements.playPauseButton.textContent = 'Play';
        }
        if (this.animationFrameId) {
            cancelAnimationFrame(this.animationFrameId);
            this.animationFrameId = null;
        }
    }

    /**
     * Handle file input change
     */
    async handleFileInput(event) {
        const file = event.target.files[0];
        if (!file) return;

        try {
            this.showLoading(true);
            const ctx = await this.initAudioContext();
            const arrayBuffer = await file.arrayBuffer();
            this.audioBuffer = await ctx.decodeAudioData(arrayBuffer);
            this.duration = this.audioBuffer.duration;
            
            if (this.elements.seekBar) {
                this.elements.seekBar.max = this.duration;
            }
            
            this.pauseOffset = 0;
            this.updateTimer(0);
            this.resetUI();
            
            // Read metadata
            await this.readMetadata(file);
            this.showLoading(false);
        } catch (error) {
            console.error('Error loading audio file:', error);
            this.showError('Error loading audio file. Please try a different file.');
            this.showLoading(false);
        }
    }

    /**
     * Read audio metadata using jsmediatags
     */
    async readMetadata(file) {
        return new Promise((resolve) => {
            if (typeof jsmediatags === 'undefined') {
                this.fallbackMetadata(file);
                resolve();
                return;
            }

            jsmediatags.read(file, {
                onSuccess: (tag) => {
                    const title = tag.tags.title || file.name;
                    if (this.elements.trackTitle) {
                        this.elements.trackTitle.textContent = title;
                    }
                    
                    if (tag.tags.picture && this.elements.coverArt) {
                        const { data, format } = tag.tags.picture;
                        const blob = new Blob([new Uint8Array(data)], { type: format });
                        this.elements.coverArt.src = URL.createObjectURL(blob);
                    } else if (this.elements.coverArt) {
                        this.elements.coverArt.src = '/images/placeholder.png';
                    }
                    resolve();
                },
                onError: () => {
                    this.fallbackMetadata(file);
                    resolve();
                }
            });
        });
    }

    /**
     * Fallback metadata when jsmediatags fails
     */
    fallbackMetadata(file) {
        if (this.elements.trackTitle) {
            this.elements.trackTitle.textContent = file.name;
        }
        if (this.elements.coverArt) {
            this.elements.coverArt.src = '/images/placeholder.png';
        }
    }

    /**
     * Toggle play/pause
     */
    async togglePlayPause() {
        if (this.isPlaying) {
            this.pauseAudio();
        } else {
            await this.playAudio();
        }
    }

    /**
     * Play audio
     */
    async playAudio() {
        if (!this.audioBuffer) return;

        try {
            const ctx = await this.initAudioContext();
            if (ctx.state === 'suspended') {
                await ctx.resume();
            }

            if (this.sourceNode) {
                this.sourceNode.stop();
                this.sourceNode.disconnect();
            }

            this.sourceNode = ctx.createBufferSource();
            this.sourceNode.buffer = this.audioBuffer;
            
            if (this.elements.speedControl) {
                this.sourceNode.playbackRate.value = parseFloat(this.elements.speedControl.value);
            }
            
            this.sourceNode.connect(ctx.destination);
            this.sourceNode.start(0, this.pauseOffset);
            
            this.startTime = ctx.currentTime - this.pauseOffset;
            this.isPlaying = true;
            
            if (this.elements.playPauseButton) {
                this.elements.playPauseButton.textContent = 'Pause';
            }
            
            this.updateSeekBar();
        } catch (error) {
            console.error('Error playing audio:', error);
            this.showError('Error playing audio. Please try again.');
            this.resetUI();
        }
    }

    /**
     * Pause audio
     */
    pauseAudio() {
        if (this.sourceNode && this.isPlaying) {
            this.sourceNode.stop();
            this.sourceNode.disconnect();
            this.sourceNode = null;
            this.pauseOffset = this.audioContext.currentTime - this.startTime;
            this.resetUI();
        }
    }

    /**
     * Handle seek bar input
     */
    handleSeek(event) {
        this.pauseOffset = parseFloat(event.target.value);
        this.updateTimer(this.pauseOffset);
        if (this.isPlaying) {
            this.playAudio();
        }
    }

    /**
     * Handle speed control change
     */
    handleSpeedChange(event) {
        const speed = parseFloat(event.target.value);
        if (this.elements.speedValue) {
            this.elements.speedValue.textContent = speed.toFixed(2) + '×';
        }
        if (this.sourceNode) {
            this.sourceNode.playbackRate.value = speed;
        }
    }

    /**
     * Handle keyboard shortcuts
     */
    handleKeyboard(event) {
        // Prevent shortcuts when typing in input fields
        if (event.target.tagName === 'INPUT') return;

        switch (event.code) {
            case 'Space':
                event.preventDefault();
                this.togglePlayPause();
                break;
            case 'ArrowLeft':
                event.preventDefault();
                this.seekRelative(-10);
                break;
            case 'ArrowRight':
                event.preventDefault();
                this.seekRelative(10);
                break;
            case 'ArrowUp':
                event.preventDefault();
                this.adjustSpeed(0.1);
                break;
            case 'ArrowDown':
                event.preventDefault();
                this.adjustSpeed(-0.1);
                break;
        }
    }

    /**
     * Seek relative to current position
     */
    seekRelative(seconds) {
        const newPosition = Math.max(0, Math.min(this.duration, this.pauseOffset + seconds));
        this.pauseOffset = newPosition;
        if (this.elements.seekBar) {
            this.elements.seekBar.value = newPosition;
        }
        this.updateTimer(newPosition);
        if (this.isPlaying) {
            this.playAudio();
        }
    }

    /**
     * Adjust playback speed
     */
    adjustSpeed(delta) {
        if (this.elements.speedControl) {
            const currentSpeed = parseFloat(this.elements.speedControl.value);
            const newSpeed = Math.max(0.5, Math.min(1.5, currentSpeed + delta));
            this.elements.speedControl.value = newSpeed;
            this.handleSpeedChange({ target: this.elements.speedControl });
        }
    }

    /**
     * Show loading state
     */
    showLoading(show) {
        if (this.elements.container) {
            this.elements.container.classList.toggle('loading', show);
        }
    }

    /**
     * Show error message
     */
    showError(message) {
        // Create or update error message element
        let errorElement = document.getElementById('error-message');
        if (!errorElement) {
            errorElement = document.createElement('div');
            errorElement.id = 'error-message';
            errorElement.className = 'error';
            this.elements.container?.appendChild(errorElement);
        }
        errorElement.textContent = message;
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            if (errorElement && errorElement.parentNode) {
                errorElement.parentNode.removeChild(errorElement);
            }
        }, 5000);
    }

    /**
     * Cleanup resources
     */
    cleanup() {
        if (this.sourceNode) {
            this.sourceNode.stop();
        }
        if (this.animationFrameId) {
            cancelAnimationFrame(this.animationFrameId);
        }
        // Clean up blob URLs
        if (this.elements.coverArt && this.elements.coverArt.src.startsWith('blob:')) {
            URL.revokeObjectURL(this.elements.coverArt.src);
        }
    }
}

// Initialize player when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    new XtalPlayer();
});

// Export for potential module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = XtalPlayer;
}
